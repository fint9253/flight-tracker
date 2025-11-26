using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.CreateTrackedFlight;

public class CreateTrackedFlightHandler : IRequestHandler<CreateTrackedFlightCommand, CreateTrackedFlightResult>
{
    private readonly ITrackedFlightRepository _repository;
    private readonly ILogger<CreateTrackedFlightHandler> _logger;

    public CreateTrackedFlightHandler(
        ITrackedFlightRepository repository,
        ILogger<CreateTrackedFlightHandler> logger)
    {
        _repository = repository;
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
            PollingIntervalMinutes = request.PollingIntervalMinutes,
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
            PollingIntervalMinutes = created.PollingIntervalMinutes,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };
    }
}
