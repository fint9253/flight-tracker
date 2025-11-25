import { useState } from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import FlightTrackingForm from './components/FlightTrackingForm';
import TrackedFlightsList from './components/TrackedFlightsList';
import { flightTrackingApi } from './services/api';
import type { CreateTrackedFlightRequest } from './types/api';
import './App.css';

function App() {
  // TODO: Replace with actual user authentication
  const userId = 'demo-user';
  const [refreshKey, setRefreshKey] = useState(0);

  const handleTrackFlight = async (data: CreateTrackedFlightRequest) => {
    await flightTrackingApi.createTrackedFlight(data);
    setRefreshKey(prev => prev + 1); // Trigger list refresh
  };

  const handleFlightDeleted = () => {
    setRefreshKey(prev => prev + 1); // Trigger list refresh
  };

  return (
    <Router>
      <div className="container">
        <header style={{ textAlign: 'center', padding: '2rem 0' }}>
          <h1>✈️ Flight Tracker</h1>
          <p style={{ color: 'var(--text-secondary)' }}>
            Track flight prices and get notified when prices drop
          </p>
        </header>

        <main>
          <FlightTrackingForm onSubmit={handleTrackFlight} userId={userId} />
          <TrackedFlightsList
            key={refreshKey}
            userId={userId}
            onFlightDeleted={handleFlightDeleted}
          />
        </main>
      </div>
    </Router>
  );
}

export default App;
