namespace FlightTracker.Core.Entities;

public class PriceHistory
{
    public Guid Id { get; set; }
    public Guid TrackedFlightId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PollTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON-serialized flight offer details including itinerary, segments, times, etc.
    /// </summary>
    public string? OfferDetailsJson { get; set; }

    // Navigation property
    public TrackedFlight TrackedFlight { get; set; } = null!;
}
