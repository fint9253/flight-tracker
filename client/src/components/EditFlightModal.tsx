import { useState } from 'react';
import type { TrackedFlight, NotificationRecipient } from '../types/api';
import { flightTrackingApi } from '../services/api';
import './EditFlightModal.css';

interface EditFlightModalProps {
  flight: TrackedFlight;
  onSave: (updates: {
    notificationThresholdPercent?: number;
    pollingIntervalHours?: number;
    dateFlexibilityDays?: number;
    maxStops?: number | null;
    isActive?: boolean;
  }) => Promise<void>;
  onClose: () => void;
}

export default function EditFlightModal({ flight, onSave, onClose }: EditFlightModalProps) {
  const [notificationThreshold, setNotificationThreshold] = useState(flight.notificationThresholdPercent);
  const [pollingInterval, setPollingInterval] = useState(flight.pollingIntervalHours);
  const [dateFlexibility, setDateFlexibility] = useState(flight.dateFlexibilityDays);
  const [maxStops, setMaxStops] = useState<number | null>(flight.maxStops);
  const [isActive, setIsActive] = useState(flight.isActive);
  const [recipients, setRecipients] = useState<NotificationRecipient[]>(flight.recipients || []);
  const [newRecipientEmail, setNewRecipientEmail] = useState('');
  const [newRecipientName, setNewRecipientName] = useState('');
  const [isAddingRecipient, setIsAddingRecipient] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleAddRecipient = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newRecipientEmail.trim()) return;

    setIsAddingRecipient(true);
    setError(null);

    try {
      const updatedFlight = await flightTrackingApi.addRecipient(flight.id, {
        email: newRecipientEmail.trim(),
        name: newRecipientName.trim() || undefined,
      });
      setRecipients(updatedFlight.recipients || []);
      setNewRecipientEmail('');
      setNewRecipientName('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add recipient');
    } finally {
      setIsAddingRecipient(false);
    }
  };

  const handleRemoveRecipient = async (recipientId: string) => {
    setError(null);

    try {
      await flightTrackingApi.removeRecipient(flight.id, recipientId);
      setRecipients(prev => prev.filter(r => r.id !== recipientId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove recipient');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSaving(true);

    try {
      await onSave({
        notificationThresholdPercent: notificationThreshold,
        pollingIntervalHours: pollingInterval,
        dateFlexibilityDays: dateFlexibility,
        maxStops,
        isActive,
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update flight');
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Edit Flight Tracking</h2>
          <button className="modal-close" onClick={onClose}>✕</button>
        </div>

        <form onSubmit={handleSubmit} className="edit-flight-form">
          {error && <div className="error-message">{error}</div>}

          <div className="form-section">
            <h3>Route (Cannot be edited)</h3>
            <div className="readonly-field">
              <span className="airport-code">{flight.departureAirportIATA}</span>
              <span className="arrow">→</span>
              <span className="airport-code">{flight.arrivalAirportIATA}</span>
            </div>
            <div className="readonly-field">
              Departure: {new Date(flight.departureDate).toLocaleDateString()}
            </div>
          </div>

          <div className="form-section">
            <h3>Tracking Settings</h3>

            <div className="form-group">
              <label htmlFor="dateFlexibility">
                Date Flexibility (days)
                <span className="field-hint">Search ±{dateFlexibility} days from departure date</span>
              </label>
              <input
                type="number"
                id="dateFlexibility"
                min="0"
                max="7"
                value={dateFlexibility}
                onChange={(e) => setDateFlexibility(Number(e.target.value))}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="maxStops">
                Maximum Stops
                <span className="field-hint">Filter flights by number of connections</span>
              </label>
              <select
                id="maxStops"
                value={maxStops === null ? 'any' : maxStops}
                onChange={(e) => setMaxStops(e.target.value === 'any' ? null : Number(e.target.value))}
              >
                <option value="any">Any number of stops</option>
                <option value="0">Direct flights only</option>
                <option value="1">Max 1 stop</option>
                <option value="2">Max 2 stops</option>
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="notificationThreshold">
                Price Alert Threshold (%)
                <span className="field-hint">Alert when price drops by this percentage</span>
              </label>
              <input
                type="number"
                id="notificationThreshold"
                min="5"
                max="50"
                value={notificationThreshold}
                onChange={(e) => setNotificationThreshold(Number(e.target.value))}
                required
              />
            </div>

            <div className="form-group">
              <label htmlFor="pollingInterval">
                Check Frequency (hours)
                <span className="field-hint">How often to check for price updates</span>
              </label>
              <input
                type="number"
                id="pollingInterval"
                min="1"
                max="24"
                value={pollingInterval}
                onChange={(e) => setPollingInterval(Number(e.target.value))}
                required
              />
            </div>

          </div>

          <div className="form-section">
            <h3>Notification Recipients</h3>

            {recipients.length > 0 ? (
              <div className="recipients-list">
                {recipients.map((recipient) => (
                  <div key={recipient.id} className="recipient-item">
                    <div className="recipient-info">
                      <span className="recipient-email">{recipient.email}</span>
                      {recipient.name && <span className="recipient-name">({recipient.name})</span>}
                    </div>
                    <button
                      type="button"
                      onClick={() => handleRemoveRecipient(recipient.id)}
                      className="btn-remove-recipient"
                      title="Remove recipient"
                    >
                      ✕
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <p className="no-recipients">No notification recipients added yet.</p>
            )}

            <form onSubmit={handleAddRecipient} className="add-recipient-form">
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="recipientEmail">Email</label>
                  <input
                    type="email"
                    id="recipientEmail"
                    value={newRecipientEmail}
                    onChange={(e) => setNewRecipientEmail(e.target.value)}
                    placeholder="recipient@example.com"
                    disabled={isAddingRecipient}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="recipientName">Name (optional)</label>
                  <input
                    type="text"
                    id="recipientName"
                    value={newRecipientName}
                    onChange={(e) => setNewRecipientName(e.target.value)}
                    placeholder="John Doe"
                    disabled={isAddingRecipient}
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn-add-recipient"
                disabled={isAddingRecipient || !newRecipientEmail.trim()}
              >
                {isAddingRecipient ? 'Adding...' : '+ Add Recipient'}
              </button>
            </form>
          </div>

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="btn-cancel" disabled={isSaving}>
              Cancel
            </button>
            <button type="submit" className="btn-save" disabled={isSaving}>
              {isSaving ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
