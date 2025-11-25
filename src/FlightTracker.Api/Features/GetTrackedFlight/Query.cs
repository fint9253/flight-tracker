using FlightTracker.Api.Features.GetTrackedFlights;
using MediatR;

namespace FlightTracker.Api.Features.GetTrackedFlight;

public record GetTrackedFlightQuery : IRequest<TrackedFlightResult?>
{
    public Guid Id { get; init; }
}
