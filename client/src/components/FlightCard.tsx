import type { TrackedFlight } from '../types/api';
import './FlightCard.css';

interface FlightCardProps {
  flight: TrackedFlight;
  onDelete: () => void;
  onToggleActive: () => void;
  onViewHistory: () => void;
}

export default function FlightCard({ flight, onDelete, onToggleActive, onViewHistory }: FlightCardProps) {
  const isPastFlight = new Date(flight.departureDate) < new Date();
  const formattedDate = new Date(flight.departureDate).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });

  const getStopsLabel = (maxStops: number | null): string => {
    if (maxStops === null) return 'Any stops';
    if (maxStops === 0) return 'Direct only';
    return `Max ${maxStops} stop${maxStops > 1 ? 's' : ''}`;
  };

  return (
    <div className={`flight-card ${!flight.isActive ? 'inactive' : ''} ${isPastFlight ? 'past' : ''}`}>
      <div className="flight-card-header">
        <div className="flight-route">
          <span className="airport">{flight.departureAirportIATA}</span>
          <span className="arrow">→</span>
          <span className="airport">{flight.arrivalAirportIATA}</span>
        </div>
      </div>

      <div className="flight-card-body">
        <div className="flight-info">
          <div className="info-item">
            <span className="label">Departure</span>
            <span className="value">{formattedDate} ±{flight.dateFlexibilityDays}d</span>
          </div>
          <div className="info-item">
            <span className="label">Stops</span>
            <span className="value">{getStopsLabel(flight.maxStops)}</span>
          </div>
          <div className="info-item">
            <span className="label">Alert Threshold</span>
            <span className="value">{flight.notificationThresholdPercent}% drop</span>
          </div>
          <div className="info-item">
            <span className="label">Check Interval</span>
            <span className="value">{flight.pollingIntervalHours}min</span>
          </div>
        </div>

        {flight.lastPolledAt && (
          <div className="last-checked">
            Last checked: {new Date(flight.lastPolledAt).toLocaleString()}
          </div>
        )}

        {flight.recipients.length > 0 && (
          <div className="recipients">
            <span className="recipients-label">📧</span>
            <span className="recipients-count">{flight.recipients.length} recipient(s)</span>
          </div>
        )}
      </div>

      <div className="flight-card-actions">
        <button
          onClick={onViewHistory}
          className="btn-view-history"
          title="View price history"
        >
          📊 Price History
        </button>
        <button
          onClick={onToggleActive}
          className={`btn-toggle ${flight.isActive ? 'active' : 'inactive'}`}
          title={flight.isActive ? 'Pause tracking' : 'Resume tracking'}
        >
          {flight.isActive ? '⏸️ Pause' : '▶️ Resume'}
        </button>
        <button
          onClick={onDelete}
          className="btn-delete"
          title="Delete tracked flight"
        >
          🗑️ Delete
        </button>
      </div>

      {!flight.isActive && (
        <div className="status-badge inactive-badge">Paused</div>
      )}
      {isPastFlight && (
        <div className="status-badge past-badge">Past Flight</div>
      )}
    </div>
  );
}
