import { useState, useEffect } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import type { PriceHistory } from '../types/api';
import { flightTrackingApi } from '../services/api';
import './PriceHistoryChart.css';

interface PriceHistoryChartProps {
  flightId: string;
  routeLabel: string;
}

interface ChartDataPoint {
  timestamp: string;
  price: number;
  formattedDate: string;
  formattedTime: string;
}

interface PriceStats {
  min: number;
  max: number;
  current: number;
  priceChange: number;
  priceChangePercent: number;
}

export default function PriceHistoryChart({ flightId, routeLabel }: PriceHistoryChartProps) {
  const [priceHistory, setPriceHistory] = useState<PriceHistory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [stats, setStats] = useState<PriceStats | null>(null);

  useEffect(() => {
    const fetchPriceHistory = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await flightTrackingApi.getPriceHistory(flightId);
        setPriceHistory(data);

        // Calculate statistics
        if (data.length > 0) {
          const prices = data.map(h => h.price);
          const min = Math.min(...prices);
          const max = Math.max(...prices);
          const current = data[data.length - 1].price;
          const initial = data[0].price;
          const priceChange = current - initial;
          const priceChangePercent = ((priceChange / initial) * 100);

          setStats({
            min,
            max,
            current,
            priceChange,
            priceChangePercent,
          });
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load price history');
      } finally {
        setLoading(false);
      }
    };

    fetchPriceHistory();
  }, [flightId]);

  // Transform data for chart
  const chartData: ChartDataPoint[] = priceHistory.map(h => {
    const date = new Date(h.pollTimestamp);
    return {
      timestamp: h.pollTimestamp,
      price: h.price,
      formattedDate: date.toLocaleDateString('en-US', {
        month: 'short',
        day: 'numeric',
      }),
      formattedTime: date.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
      }),
    };
  });

  // Custom tooltip
  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload as ChartDataPoint;
      return (
        <div className="custom-tooltip">
          <p className="tooltip-date">{data.formattedDate}</p>
          <p className="tooltip-time">{data.formattedTime}</p>
          <p className="tooltip-price">€{data.price.toFixed(2)}</p>
        </div>
      );
    }
    return null;
  };

  if (loading) {
    return (
      <div className="price-history-chart">
        <div className="chart-header">
          <h3>Price History - {routeLabel}</h3>
        </div>
        <div className="loading-state">
          <div className="spinner"></div>
          <p>Loading price history...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="price-history-chart">
        <div className="chart-header">
          <h3>Price History - {routeLabel}</h3>
        </div>
        <div className="error-state">
          <p className="error-message">{error}</p>
        </div>
      </div>
    );
  }

  if (priceHistory.length === 0) {
    return (
      <div className="price-history-chart">
        <div className="chart-header">
          <h3>Price History - {routeLabel}</h3>
        </div>
        <div className="empty-state">
          <p>No price history available yet. Check back after the first price check.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="price-history-chart">
      <div className="chart-header">
        <h3>Price History - {routeLabel}</h3>
        {stats && (
          <div className="price-stats">
            <div className="stat-item">
              <span className="stat-label">Current</span>
              <span className="stat-value current">€{stats.current.toFixed(2)}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Min</span>
              <span className="stat-value min">€{stats.min.toFixed(2)}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Max</span>
              <span className="stat-value max">€{stats.max.toFixed(2)}</span>
            </div>
            <div className="stat-item">
              <span className="stat-label">Change</span>
              <span className={`stat-value change ${stats.priceChange < 0 ? 'negative' : 'positive'}`}>
                {stats.priceChange >= 0 ? '+' : ''}€{stats.priceChange.toFixed(2)}
                <span className="change-percent">
                  ({stats.priceChangePercent >= 0 ? '+' : ''}{stats.priceChangePercent.toFixed(1)}%)
                </span>
              </span>
            </div>
          </div>
        )}
      </div>

      <div className="chart-container">
        <ResponsiveContainer width="100%" height={400}>
          <LineChart
            data={chartData}
            margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
          >
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" />
            <XAxis
              dataKey="formattedDate"
              stroke="var(--text-secondary)"
              style={{ fontSize: '0.875rem' }}
            />
            <YAxis
              stroke="var(--text-secondary)"
              style={{ fontSize: '0.875rem' }}
              tickFormatter={(value) => `€${value}`}
            />
            <Tooltip content={<CustomTooltip />} />
            <Legend />
            <Line
              type="monotone"
              dataKey="price"
              stroke="var(--primary-color)"
              strokeWidth={2}
              dot={{ fill: 'var(--primary-color)', r: 4 }}
              activeDot={{ r: 6 }}
              name="Price (€)"
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
