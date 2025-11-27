using FlightTracker.Api.Features.GetTrackedFlights;
using MediatR;

namespace FlightTracker.Api.Features.UpdateTrackedFlight;

public record UpdateTrackedFlightCommand : IRequest<TrackedFlightResult?>
{
    public Guid Id { get; init; }
    public decimal? NotificationThresholdPercent { get; init; }
    public int? PollingIntervalHours { get; init; }
    public int? DateFlexibilityDays { get; init; }
    public int? MaxStops { get; init; }
    public bool? IsActive { get; init; }
}
