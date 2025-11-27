using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Services;

public class PricePollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PricePollingService> _logger;
    private const int PollingCheckIntervalSeconds = 60; // Check every minute for flights due for polling

    public PricePollingService(
        IServiceProvider serviceProvider,
        ILogger<PricePollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Price Polling Service starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollFlightPricesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in price polling service");
            }

            // Wait before next check
            await Task.Delay(TimeSpan.FromSeconds(PollingCheckIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Price Polling Service stopping");
    }

    private async Task PollFlightPricesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var trackedFlightRepo = scope.ServiceProvider.GetRequiredService<ITrackedFlightRepository>();
        var priceHistoryRepo = scope.ServiceProvider.GetRequiredService<IPriceHistoryRepository>();
        var priceAlertRepo = scope.ServiceProvider.GetRequiredService<IPriceAlertRepository>();
        var flightPriceService = scope.ServiceProvider.GetRequiredService<IFlightPriceService>();

        var currentTime = DateTime.UtcNow;

        // Get flights that are due for polling
        var flightsDue = await trackedFlightRepo.GetFlightsDueForPollingAsync(currentTime, cancellationToken);

        // Filter out flights with past departure dates
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeFlights = flightsDue.Where(f => f.DepartureDate >= today).ToList();

        _logger.LogDebug("Found {Count} active flights due for polling", activeFlights.Count);

        foreach (var flight in activeFlights)
        {
            try
            {
                await PollSingleFlightAsync(
                    flight,
                    priceHistoryRepo,
                    priceAlertRepo,
                    flightPriceService,
                    trackedFlightRepo,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error polling route {FlightId} ({Origin} → {Destination} on {Date})",
                    flight.Id, flight.DepartureAirportIATA,
                    flight.ArrivalAirportIATA, flight.DepartureDate);
            }
        }
    }

    private async Task PollSingleFlightAsync(
        TrackedFlight flight,
        IPriceHistoryRepository priceHistoryRepo,
        IPriceAlertRepository priceAlertRepo,
        IFlightPriceService flightPriceService,
        ITrackedFlightRepository trackedFlightRepo,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Polling route {FlightId} ({Origin} → {Destination} on {Date}, ±{Flex} days, max {MaxStops} stops)",
            flight.Id, flight.DepartureAirportIATA,
            flight.ArrivalAirportIATA, flight.DepartureDate, flight.DateFlexibilityDays,
            flight.MaxStops?.ToString() ?? "any");

        // Fetch current price from API (route-based search)
        var priceData = await flightPriceService.GetRoutePriceAsync(
            flight.DepartureAirportIATA,
            flight.ArrivalAirportIATA,
            flight.DepartureDate,
            flight.DateFlexibilityDays,
            flight.MaxStops,
            cancellationToken);

        if (priceData == null)
        {
            _logger.LogWarning(
                "No price data available for route {FlightId} ({Origin} → {Destination})",
                flight.Id, flight.DepartureAirportIATA, flight.ArrivalAirportIATA);

            // Update LastPolledAt even if no data found
            flight.LastPolledAt = DateTime.UtcNow;
            await trackedFlightRepo.UpdateAsync(flight, cancellationToken);
            return;
        }

        _logger.LogInformation(
            "Route {FlightId} ({Origin} → {Destination}) current price: {Price} {Currency}",
            flight.Id, flight.DepartureAirportIATA, flight.ArrivalAirportIATA, priceData.Price, priceData.Currency);

        // Store price in history
        var priceHistory = new PriceHistory
        {
            TrackedFlightId = flight.Id,
            Price = priceData.Price,
            Currency = priceData.Currency,
            PollTimestamp = priceData.RetrievedAt
        };
        await priceHistoryRepo.AddAsync(priceHistory, cancellationToken);

        // Calculate average price
        var averagePrice = await priceHistoryRepo.GetAveragePriceAsync(flight.Id, cancellationToken);

        _logger.LogDebug(
            "Flight {FlightId} average price: {AveragePrice}, current: {CurrentPrice}",
            flight.Id, averagePrice, priceData.Price);

        // Check if alert should be created (price is below average by threshold)
        var thresholdMultiplier = 1 - (flight.NotificationThresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;

        if (priceData.Price < thresholdPrice)
        {
            var percentageChange = ((priceData.Price - averagePrice) / averagePrice) * 100;

            _logger.LogInformation(
                "Price alert triggered for route {FlightId} ({Origin} → {Destination}): " +
                "Current {CurrentPrice} is {PercentageChange:F2}% below average {AveragePrice} (threshold: {Threshold}%)",
                flight.Id, flight.DepartureAirportIATA, flight.ArrivalAirportIATA, priceData.Price, percentageChange,
                averagePrice, flight.NotificationThresholdPercent);

            var alert = new PriceAlert
            {
                TrackedFlightId = flight.Id,
                OldPrice = averagePrice,
                NewPrice = priceData.Price,
                PercentageChange = percentageChange,
                Currency = priceData.Currency,
                AlertTimestamp = DateTime.UtcNow,
                IsProcessed = false
            };

            await priceAlertRepo.AddAsync(alert, cancellationToken);

            _logger.LogInformation(
                "Created price alert {AlertId} for flight {FlightId}",
                alert.Id, flight.Id);
        }

        // Update LastPolledAt timestamp
        flight.LastPolledAt = DateTime.UtcNow;
        await trackedFlightRepo.UpdateAsync(flight, cancellationToken);

        _logger.LogDebug(
            "Successfully polled flight {FlightId}, next poll after {NextPollTime}",
            flight.Id, flight.LastPolledAt.Value.AddMinutes(flight.PollingIntervalHours));
    }
}
