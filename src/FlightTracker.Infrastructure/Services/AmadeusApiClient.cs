using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlightTracker.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Services;

public class AmadeusApiClient : IFlightPriceService, IFlightSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AmadeusApiClient> _logger;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _baseUrl;

    private const string TokenCacheKey = "amadeus_access_token";
    private const int TokenExpirySeconds = 1800; // 30 minutes
    private const int ResponseCacheTtlMinutes = 5;

    public AmadeusApiClient(
        HttpClient httpClient,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<AmadeusApiClient> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;

        _apiKey = configuration["Amadeus:ApiKey"] ?? throw new InvalidOperationException("Amadeus:ApiKey not configured");
        _apiSecret = configuration["Amadeus:ApiSecret"] ?? throw new InvalidOperationException("Amadeus:ApiSecret not configured");
        _baseUrl = configuration["Amadeus:BaseUrl"] ?? "https://api.amadeus.com";

        _httpClient.BaseAddress = new Uri(_baseUrl);
    }

    public async Task<FlightPriceData?> GetFlightPriceAsync(
        string flightNumber,
        string departureAirportIATA,
        string arrivalAirportIATA,
        DateOnly departureDate,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"flight_price_{flightNumber}_{departureAirportIATA}_{arrivalAirportIATA}_{departureDate:yyyy-MM-dd}";

        // Check cache first
        if (_cache.TryGetValue<FlightPriceData>(cacheKey, out var cachedData))
        {
            _logger.LogDebug("Returning cached flight price for {FlightNumber} on {Date}", flightNumber, departureDate);
            return cachedData;
        }

        try
        {
            // Get access token
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            // Build request URL
            var requestUrl = $"/v2/shopping/flight-offers?originLocationCode={departureAirportIATA}" +
                           $"&destinationLocationCode={arrivalAirportIATA}" +
                           $"&departureDate={departureDate:yyyy-MM-dd}" +
                           $"&adults=1" +
                           $"&max=1";

            // Make API request
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<AmadeusFlightOffersResponse>(cancellationToken);

            if (content?.Data == null || content.Data.Count == 0)
            {
                _logger.LogWarning("No flight offers found for {FlightNumber} from {Origin} to {Destination} on {Date}",
                    flightNumber, departureAirportIATA, arrivalAirportIATA, departureDate);
                return null;
            }

            // Extract price from first offer
            var offer = content.Data[0];
            var priceData = new FlightPriceData
            {
                Price = decimal.Parse(offer.Price.GrandTotal),
                Currency = offer.Price.Currency,
                RetrievedAt = DateTime.UtcNow,
                CarrierCode = offer.ValidatingAirlineCodes?.FirstOrDefault(),
                NumberOfStops = offer.Itineraries?.FirstOrDefault()?.Segments?.Count - 1 ?? 0
            };

            // Cache the result
            _cache.Set(cacheKey, priceData, TimeSpan.FromMinutes(ResponseCacheTtlMinutes));

            _logger.LogInformation("Retrieved flight price for {FlightNumber}: {Price} {Currency}",
                flightNumber, priceData.Price, priceData.Currency);

            return priceData;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching flight price for {FlightNumber}", flightNumber);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching flight price for {FlightNumber}", flightNumber);
            throw;
        }
    }

    public async Task<FlightPriceData?> GetRoutePriceAsync(
        string departureAirportIATA,
        string arrivalAirportIATA,
        DateOnly departureDate,
        int dateFlexibilityDays,
        int? maxStops,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"route_price_{departureAirportIATA}_{arrivalAirportIATA}_{departureDate:yyyy-MM-dd}_{dateFlexibilityDays}_{maxStops?.ToString() ?? "any"}";

        // Check cache first
        if (_cache.TryGetValue<FlightPriceData>(cacheKey, out var cachedData))
        {
            _logger.LogDebug("Returning cached route price for {Origin} → {Destination} on {Date} ±{Flex} days",
                departureAirportIATA, arrivalAirportIATA, departureDate, dateFlexibilityDays);
            return cachedData;
        }

        try
        {
            _logger.LogInformation("Searching route prices: {Origin} → {Destination} on {Date} ±{Flex} days, max {MaxStops} stops",
                departureAirportIATA, arrivalAirportIATA, departureDate, dateFlexibilityDays, maxStops?.ToString() ?? "any");

            // Calculate date range
            var startDate = departureDate.AddDays(-dateFlexibilityDays);
            var endDate = departureDate.AddDays(dateFlexibilityDays);

            // Get access token
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            FlightPriceData? cheapestPrice = null;

            // Search each date in the range
            for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                var requestUrl = $"/v2/shopping/flight-offers?originLocationCode={departureAirportIATA}" +
                               $"&destinationLocationCode={arrivalAirportIATA}" +
                               $"&departureDate={currentDate:yyyy-MM-dd}" +
                               $"&adults=1" +
                               $"&max=10"; // Get multiple offers to find best match

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Amadeus API request failed for date {Date}: {StatusCode}",
                        currentDate, response.StatusCode);
                    continue;
                }

                var content = await response.Content.ReadFromJsonAsync<AmadeusFlightOffersResponse>(cancellationToken);

                if (content?.Data == null || content.Data.Count == 0)
                {
                    continue;
                }

                // Filter by stops if specified
                var filteredOffers = content.Data;
                if (maxStops.HasValue)
                {
                    filteredOffers = content.Data
                        .Where(offer => {
                            var stops = offer.Itineraries?.FirstOrDefault()?.Segments?.Count - 1 ?? 0;
                            return stops <= maxStops.Value;
                        })
                        .ToList();
                }

                if (filteredOffers.Count == 0)
                {
                    continue;
                }

                // Find cheapest offer for this date
                var cheapestOffer = filteredOffers
                    .OrderBy(o => decimal.Parse(o.Price.GrandTotal))
                    .FirstOrDefault();

                if (cheapestOffer != null)
                {
                    var price = decimal.Parse(cheapestOffer.Price.GrandTotal);
                    var stops = cheapestOffer.Itineraries?.FirstOrDefault()?.Segments?.Count - 1 ?? 0;

                    // Update cheapest if this is better
                    if (cheapestPrice == null || price < cheapestPrice.Price)
                    {
                        cheapestPrice = new FlightPriceData
                        {
                            Price = price,
                            Currency = cheapestOffer.Price.Currency,
                            RetrievedAt = DateTime.UtcNow,
                            CarrierCode = cheapestOffer.ValidatingAirlineCodes?.FirstOrDefault(),
                            NumberOfStops = stops
                        };

                        _logger.LogDebug("Found cheaper option on {Date}: {Price} {Currency} ({Stops} stops)",
                            currentDate, price, cheapestOffer.Price.Currency, stops);
                    }
                }
            }

            if (cheapestPrice == null)
            {
                _logger.LogWarning("No flights found for route {Origin} → {Destination} on {Date} ±{Flex} days with max {MaxStops} stops",
                    departureAirportIATA, arrivalAirportIATA, departureDate, dateFlexibilityDays, maxStops?.ToString() ?? "any");
                return null;
            }

            // Cache the result
            _cache.Set(cacheKey, cheapestPrice, TimeSpan.FromMinutes(ResponseCacheTtlMinutes));

            _logger.LogInformation("Found cheapest route price: {Origin} → {Destination} = {Price} {Currency} ({Stops} stops)",
                departureAirportIATA, arrivalAirportIATA, cheapestPrice.Price, cheapestPrice.Currency, cheapestPrice.NumberOfStops);

            return cheapestPrice;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching route price for {Origin} → {Destination}",
                departureAirportIATA, arrivalAirportIATA);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching route price for {Origin} → {Destination}",
                departureAirportIATA, arrivalAirportIATA);
            throw;
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        // Check cache first
        if (_cache.TryGetValue<string>(TokenCacheKey, out var cachedToken))
        {
            return cachedToken!;
        }

        _logger.LogDebug("Requesting new Amadeus access token");

        // Request new token
        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/security/oauth2/token");
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _apiKey),
            new KeyValuePair<string, string>("client_secret", _apiSecret)
        });
        request.Content = content;

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<AmadeusTokenResponse>(cancellationToken);

        if (tokenResponse?.AccessToken == null)
        {
            throw new InvalidOperationException("Failed to retrieve access token from Amadeus API");
        }

        // Cache the token (with buffer to avoid expiry edge cases)
        var cacheExpiry = TimeSpan.FromSeconds(tokenResponse.ExpiresIn - 60);
        _cache.Set(TokenCacheKey, tokenResponse.AccessToken, cacheExpiry);

        _logger.LogInformation("Successfully obtained Amadeus access token, expires in {Seconds}s", tokenResponse.ExpiresIn);

        return tokenResponse.AccessToken;
    }

    public async Task<List<FlightSearchResult>> SearchFlightsAsync(
        string originIATA,
        string destinationIATA,
        DateOnly departureDateStart,
        DateOnly departureDateEnd,
        DateOnly? returnDateStart = null,
        DateOnly? returnDateEnd = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<FlightSearchResult>();

        try
        {
            var accessToken = await GetAccessTokenAsync(cancellationToken);

            // Search for each date in the range (Amadeus doesn't support date ranges directly)
            var currentDate = departureDateStart;
            while (currentDate <= departureDateEnd)
            {
                var requestUrl = $"/v2/shopping/flight-offers?originLocationCode={originIATA}" +
                               $"&destinationLocationCode={destinationIATA}" +
                               $"&departureDate={currentDate:yyyy-MM-dd}" +
                               $"&adults=1" +
                               $"&max=10"; // Get multiple offers per date

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadFromJsonAsync<AmadeusFlightOffersResponse>(cancellationToken);

                if (content?.Data != null)
                {
                    foreach (var offer in content.Data)
                    {
                        var firstSegment = offer.Itineraries?.FirstOrDefault()?.Segments?.FirstOrDefault();
                        var lastSegment = offer.Itineraries?.FirstOrDefault()?.Segments?.LastOrDefault();

                        if (firstSegment?.Departure != null && lastSegment?.Arrival != null)
                        {
                            var departureDateTime = DateTime.Parse(firstSegment.Departure.At!);
                            var arrivalDateTime = DateTime.Parse(lastSegment.Arrival.At!);

                            results.Add(new FlightSearchResult
                            {
                                FlightNumber = firstSegment.CarrierCode + firstSegment.Number,
                                AirlineCode = offer.ValidatingAirlineCodes?.FirstOrDefault() ?? firstSegment.CarrierCode ?? "",
                                OriginIATA = originIATA,
                                DestinationIATA = destinationIATA,
                                DepartureDate = DateOnly.FromDateTime(departureDateTime),
                                DepartureTime = TimeOnly.FromDateTime(departureDateTime),
                                ArrivalTime = TimeOnly.FromDateTime(arrivalDateTime),
                                Price = decimal.Parse(offer.Price.GrandTotal),
                                Currency = offer.Price.Currency,
                                NumberOfStops = (offer.Itineraries?.FirstOrDefault()?.Segments?.Count ?? 1) - 1,
                                Duration = arrivalDateTime - departureDateTime
                            });
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            _logger.LogInformation(
                "Flight search completed: {Origin} -> {Destination}, {DateStart} to {DateEnd}, found {Count} flights",
                originIATA, destinationIATA, departureDateStart, departureDateEnd, results.Count);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights from {Origin} to {Destination}",
                originIATA, destinationIATA);
            throw;
        }
    }
}

// Response DTOs
internal class AmadeusTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}

internal class AmadeusFlightOffersResponse
{
    [JsonPropertyName("data")]
    public List<FlightOffer>? Data { get; set; }
}

internal class FlightOffer
{
    [JsonPropertyName("price")]
    public PriceInfo Price { get; set; } = new();

    [JsonPropertyName("validatingAirlineCodes")]
    public List<string>? ValidatingAirlineCodes { get; set; }

    [JsonPropertyName("itineraries")]
    public List<Itinerary>? Itineraries { get; set; }
}

internal class PriceInfo
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("grandTotal")]
    public string GrandTotal { get; set; } = "0";
}

internal class Itinerary
{
    [JsonPropertyName("segments")]
    public List<Segment>? Segments { get; set; }
}

internal class Segment
{
    [JsonPropertyName("departure")]
    public LocationInfo? Departure { get; set; }

    [JsonPropertyName("arrival")]
    public LocationInfo? Arrival { get; set; }

    [JsonPropertyName("carrierCode")]
    public string? CarrierCode { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }
}

internal class LocationInfo
{
    [JsonPropertyName("iataCode")]
    public string? IataCode { get; set; }

    [JsonPropertyName("at")]
    public string? At { get; set; }
}
