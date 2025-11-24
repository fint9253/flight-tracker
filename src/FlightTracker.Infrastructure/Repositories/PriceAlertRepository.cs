using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using FlightTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightTracker.Infrastructure.Repositories;

public class PriceAlertRepository : IPriceAlertRepository
{
    private readonly FlightTrackerDbContext _context;

    public PriceAlertRepository(FlightTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<PriceAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PriceAlerts
            .Include(a => a.TrackedFlight)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PriceAlert>> GetByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default)
    {
        return await _context.PriceAlerts
            .Where(a => a.TrackedFlightId == trackedFlightId)
            .OrderByDescending(a => a.AlertTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PriceAlert>> GetUnprocessedAlertsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PriceAlerts
            .Include(a => a.TrackedFlight)
                .ThenInclude(f => f.NotificationRecipients.Where(r => r.IsActive))
            .Where(a => !a.IsProcessed)
            .OrderBy(a => a.AlertTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<PriceAlert> AddAsync(PriceAlert alert, CancellationToken cancellationToken = default)
    {
        _context.PriceAlerts.Add(alert);
        await _context.SaveChangesAsync(cancellationToken);
        return alert;
    }

    public async Task UpdateAsync(PriceAlert alert, CancellationToken cancellationToken = default)
    {
        _context.PriceAlerts.Update(alert);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
