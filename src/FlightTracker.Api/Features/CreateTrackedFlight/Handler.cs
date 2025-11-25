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
            FlightNumber = request.FlightNumber,
            DepartureAirportIATA = request.DepartureAirportIATA,
            ArrivalAirportIATA = request.ArrivalAirportIATA,
            DepartureDate = request.DepartureDate,
            NotificationThresholdPercent = request.NotificationThresholdPercent,
            PollingIntervalMinutes = request.PollingIntervalMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(flight, cancellationToken);

        _logger.LogInformation(
            "Created tracked flight {FlightId} for user {UserId}: {FlightNumber} from {Origin} to {Destination} on {Date}",
            created.Id, created.UserId, created.FlightNumber,
            created.DepartureAirportIATA, created.ArrivalAirportIATA, created.DepartureDate);

        return new CreateTrackedFlightResult
        {
            Id = created.Id,
            UserId = created.UserId,
            FlightNumber = created.FlightNumber,
            DepartureAirportIATA = created.DepartureAirportIATA,
            ArrivalAirportIATA = created.ArrivalAirportIATA,
            DepartureDate = created.DepartureDate,
            NotificationThresholdPercent = created.NotificationThresholdPercent,
            PollingIntervalMinutes = created.PollingIntervalMinutes,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };
    }
}
