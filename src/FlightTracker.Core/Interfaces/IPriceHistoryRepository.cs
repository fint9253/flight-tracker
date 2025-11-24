using FlightTracker.Core.Entities;

namespace FlightTracker.Core.Interfaces;

public interface IPriceHistoryRepository
{
    Task<PriceHistory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceHistory>> GetByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceHistory>> GetByFlightIdAsync(Guid trackedFlightId, int limit, CancellationToken cancellationToken = default);
    Task<decimal> GetAveragePriceAsync(Guid trackedFlightId, CancellationToken cancellationToken = default);
    Task<PriceHistory?> GetLatestPriceAsync(Guid trackedFlightId, CancellationToken cancellationToken = default);
    Task<PriceHistory> AddAsync(PriceHistory priceHistory, CancellationToken cancellationToken = default);
}
