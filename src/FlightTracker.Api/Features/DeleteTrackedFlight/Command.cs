using MediatR;

namespace FlightTracker.Api.Features.DeleteTrackedFlight;

public record DeleteTrackedFlightCommand : IRequest<bool>
{
    public Guid Id { get; init; }
}
