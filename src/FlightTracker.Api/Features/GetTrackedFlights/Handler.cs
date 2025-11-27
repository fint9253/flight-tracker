using FlightTracker.Core.Interfaces;
using MediatR;

namespace FlightTracker.Api.Features.GetTrackedFlights;

public class GetTrackedFlightsHandler : IRequestHandler<GetTrackedFlightsQuery, List<TrackedFlightResult>>
{
    private readonly ITrackedFlightRepository _repository;

    public GetTrackedFlightsHandler(ITrackedFlightRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TrackedFlightResult>> Handle(
        GetTrackedFlightsQuery request,
        CancellationToken cancellationToken)
    {
        var flights = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        return flights.Select(f => new TrackedFlightResult
        {
            Id = f.Id,
            UserId = f.UserId,
            DepartureAirportIATA = f.DepartureAirportIATA,
            ArrivalAirportIATA = f.ArrivalAirportIATA,
            DepartureDate = f.DepartureDate,
            DateFlexibilityDays = f.DateFlexibilityDays,
            MaxStops = f.MaxStops,
            NotificationThresholdPercent = f.NotificationThresholdPercent,
            PollingIntervalHours = f.PollingIntervalHours,
            IsActive = f.IsActive,
            LastPolledAt = f.LastPolledAt,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt,
            Recipients = f.NotificationRecipients?.Select(r => new RecipientResult
            {
                Id = r.Id,
                Email = r.Email,
                Name = r.Name,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList() ?? new List<RecipientResult>()
        }).ToList();
    }
}
