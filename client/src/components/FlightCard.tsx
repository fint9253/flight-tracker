import type { TrackedFlight } from '../types/api';
import './FlightCard.css';

interface FlightCardProps {
  flight: TrackedFlight;
  onDelete: () => void;
  onToggleActive: () => void;
}

export default function FlightCard({ flight, onDelete, onToggleActive }: FlightCardProps) {
  const isPastFlight = new Date(flight.departureDate) < new Date();
  const formattedDate = new Date(flight.departureDate).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });

  return (
    <div className={`flight-card ${!flight.isActive ? 'inactive' : ''} ${isPastFlight ? 'past' : ''}`}>
      <div className="flight-card-header">
        <div className="flight-route">
          <span className="airport">{flight.departureAirportIATA}</span>
          <span className="arrow">→</span>
          <span className="airport">{flight.arrivalAirportIATA}</span>
        </div>
        <span className="flight-number">{flight.flightNumber}</span>
      </div>

      <div className="flight-card-body">
        <div className="flight-info">
          <div className="info-item">
            <span className="label">Departure</span>
            <span className="value">{formattedDate}</span>
          </div>
          <div className="info-item">
            <span className="label">Alert Threshold</span>
            <span className="value">{flight.notificationThresholdPercent}% drop</span>
          </div>
          <div className="info-item">
            <span className="label">Check Interval</span>
            <span className="value">{flight.pollingIntervalMinutes}min</span>
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
