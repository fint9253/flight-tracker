using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using FlightTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightTracker.Infrastructure.Repositories;

public class NotificationRecipientRepository : INotificationRecipientRepository
{
    private readonly FlightTrackerDbContext _context;

    public NotificationRecipientRepository(FlightTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationRecipient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationRecipients
            .Include(r => r.TrackedFlight)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<NotificationRecipient>> GetByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationRecipients
            .Where(r => r.TrackedFlightId == trackedFlightId)
            .OrderBy(r => r.Email)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificationRecipient>> GetActiveByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationRecipients
            .Where(r => r.TrackedFlightId == trackedFlightId && r.IsActive)
            .OrderBy(r => r.Email)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NotificationRecipient>> GetAllWithFlightsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.NotificationRecipients
            .Include(r => r.TrackedFlight)
            .Where(r => r.IsActive && r.TrackedFlight.IsActive)
            .OrderBy(r => r.Email)
            .ThenBy(r => r.TrackedFlight.DepartureDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationRecipient> AddAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default)
    {
        _context.NotificationRecipients.Add(recipient);
        await _context.SaveChangesAsync(cancellationToken);
        return recipient;
    }

    public async Task UpdateAsync(NotificationRecipient recipient, CancellationToken cancellationToken = default)
    {
        _context.NotificationRecipients.Update(recipient);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var recipient = await _context.NotificationRecipients.FindAsync([id], cancellationToken);
        if (recipient != null)
        {
            _context.NotificationRecipients.Remove(recipient);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
