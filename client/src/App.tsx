import { BrowserRouter as Router } from 'react-router-dom';
import './App.css';

function App() {
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
          <div className="card" style={{ textAlign: 'center', margin: '2rem 0' }}>
            <h2>Welcome!</h2>
            <p>React app is set up and ready for development.</p>
            <p style={{ marginTop: '1rem', color: 'var(--text-secondary)' }}>
              API proxy configured to forward /api requests to localhost:5000
            </p>
          </div>
        </main>
      </div>
    </Router>
  );
}

export default App;
