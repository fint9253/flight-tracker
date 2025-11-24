using FlightTracker.Core.Entities;

namespace FlightTracker.Core.Interfaces;

public interface ITrackedFlightRepository
{
    Task<TrackedFlight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrackedFlight>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TrackedFlight>> GetActiveFlightsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TrackedFlight>> GetFlightsDueForPollingAsync(DateTime currentTime, CancellationToken cancellationToken = default);
    Task<TrackedFlight> AddAsync(TrackedFlight flight, CancellationToken cancellationToken = default);
    Task UpdateAsync(TrackedFlight flight, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
