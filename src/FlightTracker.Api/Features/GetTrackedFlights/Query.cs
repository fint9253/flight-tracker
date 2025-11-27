using MediatR;

namespace FlightTracker.Api.Features.GetTrackedFlights;

public record GetTrackedFlightsQuery : IRequest<List<TrackedFlightResult>>
{
    public string UserId { get; init; } = string.Empty;
}

public record TrackedFlightResult
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; }
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; }
    public int PollingIntervalHours { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastPolledAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<RecipientResult> Recipients { get; init; } = new();
}

public record RecipientResult
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
