using MediatR;

namespace FlightTracker.Api.Features.BatchCreateTrackedFlights;

public record BatchCreateTrackedFlightsCommand : IRequest<BatchCreateTrackedFlightsResult>
{
    public List<FlightToTrack> Flights { get; init; } = new();
}

public record FlightToTrack
{
    public string UserId { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; } = 3;
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; } = 5.00m;
    public int PollingIntervalHours { get; init; } = 15;
}

public record BatchCreateTrackedFlightsResult
{
    public int TotalRequested { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<BatchFlightResult> Results { get; init; } = new();
}

public record BatchFlightResult
{
    public int Index { get; init; }
    public bool Success { get; init; }
    public Guid? FlightId { get; init; }
    public string? Route { get; init; }
    public string? ErrorMessage { get; init; }
}
