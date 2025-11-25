import { useState, useEffect } from 'react';
import type { TrackedFlight } from '../types/api';
import { flightTrackingApi } from '../services/api';
import FlightCard from './FlightCard';
import PriceHistoryChart from './PriceHistoryChart';
import './TrackedFlightsList.css';

interface TrackedFlightsListProps {
  userId: string;
  onFlightDeleted?: () => void;
}

export default function TrackedFlightsList({ userId, onFlightDeleted }: TrackedFlightsListProps) {
  const [flights, setFlights] = useState<TrackedFlight[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedFlightId, setSelectedFlightId] = useState<string | null>(null);

  const fetchFlights = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await flightTrackingApi.getTrackedFlights(userId);
      setFlights(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load flights');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFlights();
  }, [userId]);

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to stop tracking this flight?')) {
      return;
    }

    try {
      await flightTrackingApi.deleteTrackedFlight(id);
      setFlights(prev => prev.filter(f => f.id !== id));
      onFlightDeleted?.();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to delete flight');
    }
  };

  const handleToggleActive = async (id: string, isActive: boolean) => {
    try {
      const updated = await flightTrackingApi.updateTrackedFlight(id, { isActive });
      setFlights(prev => prev.map(f => f.id === id ? updated : f));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to update flight');
    }
  };

  const handleViewHistory = (flightId: string) => {
    setSelectedFlightId(selectedFlightId === flightId ? null : flightId);
  };

  if (loading) {
    return (
      <div className="tracked-flights-list">
        <div className="loading-state">
          <div className="spinner"></div>
          <p>Loading your tracked flights...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="tracked-flights-list">
        <div className="error-state">
          <p className="error-message">{error}</p>
          <button onClick={fetchFlights}>Retry</button>
        </div>
      </div>
    );
  }

  if (flights.length === 0) {
    return (
      <div className="tracked-flights-list">
        <div className="empty-state">
          <h3>No flights tracked yet</h3>
          <p>Start tracking a flight above to get price alerts!</p>
        </div>
      </div>
    );
  }

  const activeFlights = flights.filter(f => f.isActive);
  const inactiveFlights = flights.filter(f => !f.isActive);

  return (
    <div className="tracked-flights-list">
      <div className="list-header">
        <h2>Your Tracked Flights</h2>
        <div className="stats">
          <span className="stat">
            <strong>{flights.length}</strong> total
          </span>
          <span className="stat">
            <strong>{activeFlights.length}</strong> active
          </span>
        </div>
      </div>

      {activeFlights.length > 0 && (
        <section className="flights-section">
          <h3>Active</h3>
          <div className="flights-grid">
            {activeFlights.map(flight => (
              <FlightCard
                key={flight.id}
                flight={flight}
                onDelete={() => handleDelete(flight.id)}
                onToggleActive={() => handleToggleActive(flight.id, !flight.isActive)}
                onViewHistory={() => handleViewHistory(flight.id)}
              />
            ))}
          </div>
        </section>
      )}

      {inactiveFlights.length > 0 && (
        <section className="flights-section">
          <h3>Inactive</h3>
          <div className="flights-grid">
            {inactiveFlights.map(flight => (
              <FlightCard
                key={flight.id}
                flight={flight}
                onDelete={() => handleDelete(flight.id)}
                onToggleActive={() => handleToggleActive(flight.id, !flight.isActive)}
                onViewHistory={() => handleViewHistory(flight.id)}
              />
            ))}
          </div>
        </section>
      )}

      {selectedFlightId && (
        <PriceHistoryChart
          flightId={selectedFlightId}
          flightNumber={flights.find(f => f.id === selectedFlightId)?.flightNumber || 'Unknown'}
        />
      )}
    </div>
  );
}
