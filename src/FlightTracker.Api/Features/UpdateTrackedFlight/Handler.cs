using FlightTracker.Api.Features.GetTrackedFlights;
using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.UpdateTrackedFlight;

public class UpdateTrackedFlightHandler : IRequestHandler<UpdateTrackedFlightCommand, TrackedFlightResult?>
{
    private readonly ITrackedFlightRepository _repository;
    private readonly ILogger<UpdateTrackedFlightHandler> _logger;

    public UpdateTrackedFlightHandler(
        ITrackedFlightRepository repository,
        ILogger<UpdateTrackedFlightHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TrackedFlightResult?> Handle(
        UpdateTrackedFlightCommand request,
        CancellationToken cancellationToken)
    {
        var flight = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (flight == null)
        {
            return null;
        }

        if (request.NotificationThresholdPercent.HasValue)
        {
            flight.NotificationThresholdPercent = request.NotificationThresholdPercent.Value;
        }

        if (request.PollingIntervalMinutes.HasValue)
        {
            flight.PollingIntervalMinutes = request.PollingIntervalMinutes.Value;
        }

        if (request.IsActive.HasValue)
        {
            flight.IsActive = request.IsActive.Value;
        }

        flight.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(flight, cancellationToken);

        _logger.LogInformation("Updated tracked flight {FlightId}", request.Id);

        return new TrackedFlightResult
        {
            Id = flight.Id,
            UserId = flight.UserId,
            DepartureAirportIATA = flight.DepartureAirportIATA,
            ArrivalAirportIATA = flight.ArrivalAirportIATA,
            DepartureDate = flight.DepartureDate,
            DateFlexibilityDays = flight.DateFlexibilityDays,
            MaxStops = flight.MaxStops,
            NotificationThresholdPercent = flight.NotificationThresholdPercent,
            PollingIntervalMinutes = flight.PollingIntervalMinutes,
            IsActive = flight.IsActive,
            LastPolledAt = flight.LastPolledAt,
            CreatedAt = flight.CreatedAt,
            UpdatedAt = flight.UpdatedAt,
            Recipients = flight.NotificationRecipients?.Select(r => new RecipientResult
            {
                Id = r.Id,
                Email = r.Email,
                Name = r.Name,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList() ?? new List<RecipientResult>()
        };
    }
}
