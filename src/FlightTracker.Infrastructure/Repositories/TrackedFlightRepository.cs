using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using FlightTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightTracker.Infrastructure.Repositories;

public class TrackedFlightRepository : ITrackedFlightRepository
{
    private readonly FlightTrackerDbContext _context;

    public TrackedFlightRepository(FlightTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<TrackedFlight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Include(f => f.PriceHistories)
            .Include(f => f.PriceAlerts)
            .Include(f => f.NotificationRecipients)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<TrackedFlight>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Include(f => f.PriceHistories.OrderByDescending(p => p.PollTimestamp).Take(10))
            .Include(f => f.NotificationRecipients)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TrackedFlight>> GetActiveFlightsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Where(f => f.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TrackedFlight>> GetFlightsDueForPollingAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await _context.TrackedFlights
            .Where(f => f.IsActive &&
                       (f.LastPolledAt == null ||
                        f.LastPolledAt.Value.AddMinutes(f.PollingIntervalMinutes) <= currentTime))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrackedFlight> AddAsync(TrackedFlight flight, CancellationToken cancellationToken = default)
    {
        _context.TrackedFlights.Add(flight);
        await _context.SaveChangesAsync(cancellationToken);
        return flight;
    }

    public async Task UpdateAsync(TrackedFlight flight, CancellationToken cancellationToken = default)
    {
        flight.UpdatedAt = DateTime.UtcNow;
        _context.TrackedFlights.Update(flight);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flight = await _context.TrackedFlights.FindAsync([id], cancellationToken);
        if (flight != null)
        {
            _context.TrackedFlights.Remove(flight);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
