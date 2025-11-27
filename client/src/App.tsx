import { useState, useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { SignedIn, SignedOut, UserButton, useUser, useAuth } from '@clerk/clerk-react';
import FlightTrackingForm from './components/FlightTrackingForm';
import TrackedFlightsList from './components/TrackedFlightsList';
import SignInPage from './pages/SignInPage';
import SignUpPage from './pages/SignUpPage';
import { flightTrackingApi, configureApiClient } from './services/api';
import type { CreateTrackedFlightRequest } from './types/api';
import './App.css';

interface Recipient {
  email: string;
  name?: string;
}

function Dashboard() {
  const { user } = useUser();
  const { getToken } = useAuth();
  const [refreshKey, setRefreshKey] = useState(0);

  // Configure API client with Clerk's token getter
  useEffect(() => {
    configureApiClient(getToken);
  }, [getToken]);

  // Get user ID from Clerk authentication
  const userId = user?.id || '';

  const handleTrackFlight = async (data: CreateTrackedFlightRequest, recipients: Recipient[]) => {
    const flight = await flightTrackingApi.createTrackedFlight(data);

    // Add recipients to the tracked flight
    for (const recipient of recipients) {
      await flightTrackingApi.addRecipient(flight.id, recipient);
    }

    setRefreshKey(prev => prev + 1); // Trigger list refresh
  };

  const handleFlightDeleted = () => {
    setRefreshKey(prev => prev + 1); // Trigger list refresh
  };

  if (!userId) {
    return <div>Loading...</div>;
  }

  return (
    <div className="container">
      <header style={{
        textAlign: 'center',
        padding: '2rem 0',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center'
      }}>
        <div style={{ flex: 1 }}>
          <h1>✈️ Flight Tracker</h1>
          <p style={{ color: 'var(--text-secondary)' }}>
            Track flight prices and get notified when prices drop
          </p>
        </div>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '1rem',
          paddingRight: '1rem'
        }}>
          <span style={{ color: 'var(--text-secondary)' }}>
            {user?.firstName || user?.emailAddresses[0]?.emailAddress}
          </span>
          <UserButton afterSignOutUrl="/sign-in" />
        </div>
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
  );
}

function App() {
  return (
    <Router>
      <Routes>
        {/* Public routes */}
        <Route path="/sign-in/*" element={<SignInPage />} />
        <Route path="/sign-up/*" element={<SignUpPage />} />

        {/* Protected routes */}
        <Route
          path="/"
          element={
            <>
              <SignedIn>
                <Dashboard />
              </SignedIn>
              <SignedOut>
                <Navigate to="/sign-in" replace />
              </SignedOut>
            </>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
