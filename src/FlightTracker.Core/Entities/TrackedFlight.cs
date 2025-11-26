namespace FlightTracker.Core.Entities;

public class TrackedFlight
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DepartureAirportIATA { get; set; } = string.Empty;
    public string ArrivalAirportIATA { get; set; } = string.Empty;
    public DateOnly DepartureDate { get; set; }
    public int DateFlexibilityDays { get; set; } = 3; // ±3 days
    public int? MaxStops { get; set; } // null = any, 0 = direct, 1 = 1 stop, 2+ = 2+ stops
    public decimal NotificationThresholdPercent { get; set; } = 5.00m;
    public int PollingIntervalMinutes { get; set; } = 15;
    public bool IsActive { get; set; } = true;
    public DateTime? LastPolledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    public ICollection<PriceAlert> PriceAlerts { get; set; } = new List<PriceAlert>();
    public ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
}
