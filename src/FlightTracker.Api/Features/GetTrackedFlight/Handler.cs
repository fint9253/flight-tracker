using FlightTracker.Api.Features.GetTrackedFlights;
using FlightTracker.Core.Interfaces;
using MediatR;

namespace FlightTracker.Api.Features.GetTrackedFlight;

public class GetTrackedFlightHandler : IRequestHandler<GetTrackedFlightQuery, TrackedFlightResult?>
{
    private readonly ITrackedFlightRepository _repository;

    public GetTrackedFlightHandler(ITrackedFlightRepository repository)
    {
        _repository = repository;
    }

    public async Task<TrackedFlightResult?> Handle(
        GetTrackedFlightQuery request,
        CancellationToken cancellationToken)
    {
        var flight = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (flight == null)
        {
            return null;
        }

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
            PollingIntervalHours = flight.PollingIntervalHours,
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
