using MediatR;

namespace FlightTracker.Api.Features.CreateTrackedFlight;

public record CreateTrackedFlightCommand : IRequest<CreateTrackedFlightResult>
{
    public string UserId { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; } = 3;
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; } = 5.00m;
    public int PollingIntervalMinutes { get; init; } = 15;
}

public record CreateTrackedFlightResult
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; }
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; }
    public int PollingIntervalMinutes { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
