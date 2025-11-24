namespace FlightTracker.Core.Entities;

public class PriceAlert
{
    public Guid Id { get; set; }
    public Guid TrackedFlightId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal PercentageChange { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime AlertTimestamp { get; set; } = DateTime.UtcNow;
    public bool IsProcessed { get; set; } = false;
    public DateTime? ProcessedAt { get; set; }

    // Navigation property
    public TrackedFlight TrackedFlight { get; set; } = null!;
}
