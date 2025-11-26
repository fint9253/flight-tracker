import { useState } from 'react';
import type { FormEvent } from 'react';
import type { CreateTrackedFlightRequest } from '../types/api';
import './FlightTrackingForm.css';

interface FlightTrackingFormProps {
  onSubmit: (data: CreateTrackedFlightRequest) => Promise<void>;
  userId: string;
}

export default function FlightTrackingForm({ onSubmit, userId }: FlightTrackingFormProps) {
  const [formData, setFormData] = useState<CreateTrackedFlightRequest>({
    userId,
    departureAirportIATA: '',
    arrivalAirportIATA: '',
    departureDate: '',
    dateFlexibilityDays: 3,
    maxStops: null,
    notificationThresholdPercent: 5,
    pollingIntervalMinutes: 15,
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);
    setSuccess(false);
    setLoading(true);

    try {
      await onSubmit(formData);
      setSuccess(true);
      // Reset form
      setFormData({
        userId,
        departureAirportIATA: '',
        arrivalAirportIATA: '',
        departureDate: '',
        dateFlexibilityDays: 3,
        maxStops: null,
        notificationThresholdPercent: 5,
        pollingIntervalMinutes: 15,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to track route');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field: keyof CreateTrackedFlightRequest, value: string | number | null) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };

  return (
    <div className="flight-tracking-form-container">
      <h2>Track a Route</h2>
      <p className="form-description">
        Track the cheapest flights on a route and get notified of price drops
      </p>

      <form onSubmit={handleSubmit} className="flight-tracking-form">
        <div className="form-row">
          <div className="form-group">
            <label htmlFor="departureDate">
              Departure Date <span className="required">*</span>
            </label>
            <input
              id="departureDate"
              type="date"
              value={formData.departureDate}
              onChange={(e) => handleChange('departureDate', e.target.value)}
              min={new Date().toISOString().split('T')[0]}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="dateFlexibilityDays">
              Date Flexibility (days)
            </label>
            <input
              id="dateFlexibilityDays"
              type="number"
              min="0"
              max="7"
              value={formData.dateFlexibilityDays}
              onChange={(e) => handleChange('dateFlexibilityDays', Number(e.target.value))}
            />
            <small>Search ±{formData.dateFlexibilityDays} days around departure date</small>
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="maxStops">
              Maximum Stops
            </label>
            <select
              id="maxStops"
              value={formData.maxStops === null ? 'any' : formData.maxStops}
              onChange={(e) => handleChange('maxStops', e.target.value === 'any' ? null : Number(e.target.value))}
            >
              <option value="any">Any number of stops</option>
              <option value="0">Direct flights only</option>
              <option value="1">1 stop maximum</option>
              <option value="2">2 stops maximum</option>
            </select>
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="departureAirport">
              Departure Airport <span className="required">*</span>
            </label>
            <input
              id="departureAirport"
              type="text"
              placeholder="e.g., DUB"
              value={formData.departureAirportIATA}
              onChange={(e) => handleChange('departureAirportIATA', e.target.value.toUpperCase())}
              required
              pattern="^[A-Z]{3}$"
              maxLength={3}
              title="3-letter IATA code (e.g., DUB)"
            />
          </div>

          <div className="form-group">
            <label htmlFor="arrivalAirport">
              Arrival Airport <span className="required">*</span>
            </label>
            <input
              id="arrivalAirport"
              type="text"
              placeholder="e.g., MAD"
              value={formData.arrivalAirportIATA}
              onChange={(e) => handleChange('arrivalAirportIATA', e.target.value.toUpperCase())}
              required
              pattern="^[A-Z]{3}$"
              maxLength={3}
              title="3-letter IATA code (e.g., MAD)"
            />
          </div>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="threshold">
              Price Drop Alert Threshold (%)
            </label>
            <input
              id="threshold"
              type="number"
              min="1"
              max="100"
              value={formData.notificationThresholdPercent}
              onChange={(e) => handleChange('notificationThresholdPercent', Number(e.target.value))}
            />
            <small>Get notified when price drops by this percentage</small>
          </div>

          <div className="form-group">
            <label htmlFor="pollingInterval">
              Check Frequency (minutes)
            </label>
            <input
              id="pollingInterval"
              type="number"
              min="5"
              max="1440"
              value={formData.pollingIntervalMinutes}
              onChange={(e) => handleChange('pollingIntervalMinutes', Number(e.target.value))}
            />
            <small>How often to check for price updates</small>
          </div>
        </div>

        {error && (
          <div className="alert alert-error">
            {error}
          </div>
        )}

        {success && (
          <div className="alert alert-success">
            Route tracking started successfully!
          </div>
        )}

        <button type="submit" disabled={loading} className="submit-button">
          {loading ? 'Starting Tracking...' : 'Start Tracking Route'}
        </button>
      </form>
    </div>
  );
}
