namespace FlightTracker.Core.Entities;

public class NotificationRecipient
{
    public Guid Id { get; set; }
    public Guid TrackedFlightId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public TrackedFlight TrackedFlight { get; set; } = null!;
}
