using MediatR;

namespace FlightTracker.Api.Features.GetRecipientSummary;

public record GetRecipientSummaryQuery : IRequest<RecipientSummaryResponse>
{
}

public record RecipientSummaryResponse
{
    public List<RecipientSummary> Recipients { get; init; } = new();
}

public record RecipientSummary
{
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public List<RecipientFlight> TrackedFlights { get; init; } = new();
}

public record RecipientFlight
{
    public Guid Id { get; init; }
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

    // Price data
    public decimal? CurrentPrice { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public decimal? PriceChangePercent { get; init; }
    public string? Currency { get; init; }
    public List<PricePoint> PriceHistory { get; init; } = new();
}

public record PricePoint
{
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PollTimestamp { get; init; }
}
