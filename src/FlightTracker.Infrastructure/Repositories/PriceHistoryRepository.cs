using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using FlightTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightTracker.Infrastructure.Repositories;

public class PriceHistoryRepository : IPriceHistoryRepository
{
    private readonly FlightTrackerDbContext _context;

    public PriceHistoryRepository(FlightTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<PriceHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PriceHistories
            .Include(p => p.TrackedFlight)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PriceHistory>> GetByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default)
    {
        return await _context.PriceHistories
            .Where(p => p.TrackedFlightId == trackedFlightId)
            .OrderByDescending(p => p.PollTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PriceHistory>> GetByFlightIdAsync(Guid trackedFlightId, int limit, CancellationToken cancellationToken = default)
    {
        return await _context.PriceHistories
            .Where(p => p.TrackedFlightId == trackedFlightId)
            .OrderByDescending(p => p.PollTimestamp)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetAveragePriceAsync(Guid trackedFlightId, CancellationToken cancellationToken = default)
    {
        var prices = await _context.PriceHistories
            .Where(p => p.TrackedFlightId == trackedFlightId)
            .Select(p => p.Price)
            .ToListAsync(cancellationToken);

        return prices.Any() ? prices.Average() : 0;
    }

    public async Task<PriceHistory?> GetLatestPriceAsync(Guid trackedFlightId, CancellationToken cancellationToken = default)
    {
        return await _context.PriceHistories
            .Where(p => p.TrackedFlightId == trackedFlightId)
            .OrderByDescending(p => p.PollTimestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PriceHistory> AddAsync(PriceHistory priceHistory, CancellationToken cancellationToken = default)
    {
        _context.PriceHistories.Add(priceHistory);
        await _context.SaveChangesAsync(cancellationToken);
        return priceHistory;
    }
}
