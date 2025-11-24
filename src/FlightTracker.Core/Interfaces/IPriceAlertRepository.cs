using FlightTracker.Core.Entities;

namespace FlightTracker.Core.Interfaces;

public interface IPriceAlertRepository
{
    Task<PriceAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceAlert>> GetByFlightIdAsync(Guid trackedFlightId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PriceAlert>> GetUnprocessedAlertsAsync(CancellationToken cancellationToken = default);
    Task<PriceAlert> AddAsync(PriceAlert alert, CancellationToken cancellationToken = default);
    Task UpdateAsync(PriceAlert alert, CancellationToken cancellationToken = default);
}
