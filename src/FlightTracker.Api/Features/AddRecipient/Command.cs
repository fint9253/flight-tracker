using MediatR;

namespace FlightTracker.Api.Features.AddRecipient;

public record AddRecipientCommand : IRequest<AddRecipientResult>
{
    public Guid TrackedFlightId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
}

public record AddRecipientResult
{
    public bool FlightExists { get; init; }
    public RecipientData? Recipient { get; init; }
}

public record RecipientData
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
