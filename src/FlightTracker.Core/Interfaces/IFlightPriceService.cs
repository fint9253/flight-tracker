namespace FlightTracker.Core.Interfaces;

public interface IFlightPriceService
{
    /// <summary>
    /// Retrieves the current price for a specific flight.
    /// </summary>
    /// <param name="flightNumber">Flight number (e.g., "AA123")</param>
    /// <param name="departureAirportIATA">Departure airport IATA code (e.g., "JFK")</param>
    /// <param name="arrivalAirportIATA">Arrival airport IATA code (e.g., "LAX")</param>
    /// <param name="departureDate">Date of departure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight price data or null if not found</returns>
    Task<FlightPriceData?> GetFlightPriceAsync(
        string flightNumber,
        string departureAirportIATA,
        string arrivalAirportIATA,
        DateOnly departureDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for the best price for a route with flexible dates and stop preferences.
    /// </summary>
    /// <param name="departureAirportIATA">Departure airport IATA code (e.g., "JFK")</param>
    /// <param name="arrivalAirportIATA">Arrival airport IATA code (e.g., "LAX")</param>
    /// <param name="departureDate">Preferred departure date</param>
    /// <param name="dateFlexibilityDays">Number of days flexibility (±days)</param>
    /// <param name="maxStops">Maximum number of stops (null = any, 0 = direct, 1 = 1 stop, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best flight price data for the route or null if not found</returns>
    Task<FlightPriceData?> GetRoutePriceAsync(
        string departureAirportIATA,
        string arrivalAirportIATA,
        DateOnly departureDate,
        int dateFlexibilityDays,
        int? maxStops,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Flight price data returned by the flight price service.
/// </summary>
public record FlightPriceData
{
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
    public string? CarrierCode { get; init; }
    public int NumberOfStops { get; init; }
    public FlightOfferDetails? OfferDetails { get; init; }
}

/// <summary>
/// Detailed flight offer information including itinerary and segments.
/// </summary>
public record FlightOfferDetails
{
    public DateOnly DepartureDate { get; init; }
    public DateTime DepartureDateTime { get; init; }
    public DateTime ArrivalDateTime { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public List<FlightSegmentDetails> Segments { get; init; } = new();
}

/// <summary>
/// Individual flight segment (leg) information.
/// </summary>
public record FlightSegmentDetails
{
    public string DepartureAirport { get; init; } = string.Empty;
    public string ArrivalAirport { get; init; } = string.Empty;
    public DateTime DepartureTime { get; init; }
    public DateTime ArrivalTime { get; init; }
    public TimeSpan Duration { get; init; }
    public string CarrierCode { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public TimeSpan? LayoverDuration { get; init; } // Time until next segment
}
