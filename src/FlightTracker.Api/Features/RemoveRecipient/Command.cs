using MediatR;

namespace FlightTracker.Api.Features.RemoveRecipient;

public record RemoveRecipientCommand : IRequest<RemoveRecipientResult>
{
    public Guid TrackedFlightId { get; init; }
    public Guid RecipientId { get; init; }
}

public record RemoveRecipientResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}
