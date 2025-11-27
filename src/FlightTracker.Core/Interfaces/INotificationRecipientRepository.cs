using FlightTracker.Core.Entities;

namespace FlightTracker.Core.Interfaces;

public interface INotificationRecipientRepository
{
    Task<NotificationRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationRecipient>> GetByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationRecipient>> GetActiveByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default);
    Task<IEnumerable<NotificationRecipient>> GetAllWithFlightsAsync(CancellationToken cancellationToken = default);
    Task<NotificationRecipient> AddAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
