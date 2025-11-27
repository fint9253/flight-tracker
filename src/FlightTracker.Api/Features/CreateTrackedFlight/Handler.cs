using System.Text.Json;
using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.CreateTrackedFlight;

public class CreateTrackedFlightHandler : IRequestHandler<CreateTrackedFlightCommand, CreateTrackedFlightResult>
{
    private readonly ITrackedFlightRepository _repository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;
    private readonly IFlightPriceService _flightPriceService;
    private readonly ILogger<CreateTrackedFlightHandler> _logger;

    public CreateTrackedFlightHandler(
        ITrackedFlightRepository repository,
        IPriceHistoryRepository priceHistoryRepository,
        IFlightPriceService flightPriceService,
        ILogger<CreateTrackedFlightHandler> logger)
    {
        _repository = repository;
        _priceHistoryRepository = priceHistoryRepository;
        _flightPriceService = flightPriceService;
        _logger = logger;
    }

    public async Task<CreateTrackedFlightResult> Handle(
        CreateTrackedFlightCommand request,
        CancellationToken cancellationToken)
    {
        var flight = new TrackedFlight
        {
            UserId = request.UserId,
            DepartureAirportIATA = request.DepartureAirportIATA,
            ArrivalAirportIATA = request.ArrivalAirportIATA,
            DepartureDate = request.DepartureDate,
            DateFlexibilityDays = request.DateFlexibilityDays,
            MaxStops = request.MaxStops,
            NotificationThresholdPercent = request.NotificationThresholdPercent,
            PollingIntervalHours = request.PollingIntervalHours,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(flight, cancellationToken);

        _logger.LogInformation(
            "Created tracked flight {FlightId} for user {UserId}: {Origin} → {Destination} on {Date} ±{Flex} days, max {MaxStops} stops",
            created.Id, created.UserId,
            created.DepartureAirportIATA, created.ArrivalAirportIATA, created.DepartureDate,
            created.DateFlexibilityDays, created.MaxStops?.ToString() ?? "any");

        // Immediately fetch first price to populate data
        try
        {
            var priceData = await _flightPriceService.GetRoutePriceAsync(
                created.DepartureAirportIATA,
                created.ArrivalAirportIATA,
                created.DepartureDate,
                created.DateFlexibilityDays,
                created.MaxStops,
                cancellationToken);

            if (priceData != null)
            {
                var priceHistory = new PriceHistory
                {
                    TrackedFlightId = created.Id,
                    Price = priceData.Price,
                    Currency = priceData.Currency,
                    PollTimestamp = priceData.RetrievedAt,
                    OfferDetailsJson = priceData.OfferDetails != null
                        ? JsonSerializer.Serialize(priceData.OfferDetails)
                        : null
                };
                await _priceHistoryRepository.AddAsync(priceHistory, cancellationToken);

                created.LastPolledAt = DateTime.UtcNow;
                await _repository.UpdateAsync(created, cancellationToken);

                _logger.LogInformation(
                    "Initial price fetched for flight {FlightId}: {Price} {Currency}",
                    created.Id, priceData.Price, priceData.Currency);
            }
            else
            {
                _logger.LogWarning(
                    "No initial price data available for flight {FlightId}",
                    created.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch initial price for flight {FlightId}", created.Id);
            // Don't fail the request if price fetch fails
        }

        return new CreateTrackedFlightResult
        {
            Id = created.Id,
            UserId = created.UserId,
            DepartureAirportIATA = created.DepartureAirportIATA,
            ArrivalAirportIATA = created.ArrivalAirportIATA,
            DepartureDate = created.DepartureDate,
            DateFlexibilityDays = created.DateFlexibilityDays,
            MaxStops = created.MaxStops,
            NotificationThresholdPercent = created.NotificationThresholdPercent,
            PollingIntervalHours = created.PollingIntervalHours,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };
    }
}
