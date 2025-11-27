import axios from 'axios';
import type {
  TrackedFlight,
  CreateTrackedFlightRequest,
  FlightSearchResult,
  SearchFlightsRequest,
  PriceHistory,
  RouteGroup,
} from '../types/api';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Global token getter function (will be set by App.tsx)
let getTokenFunction: (() => Promise<string | null>) | null = null;

// Function to configure the API client with Clerk's token getter
export const configureApiClient = (getToken: () => Promise<string | null>) => {
  getTokenFunction = getToken;
};

// Request interceptor to add Clerk JWT token to all requests
api.interceptors.request.use(
  async (config) => {
    if (getTokenFunction) {
      try {
        const token = await getTokenFunction();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
      } catch (error) {
        console.error('Failed to get auth token:', error);
      }
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Flight Tracking API
export const flightTrackingApi = {
  // Create a new tracked flight
  createTrackedFlight: async (data: CreateTrackedFlightRequest): Promise<TrackedFlight> => {
    const response = await api.post<TrackedFlight>('/tracking', data);
    return response.data;
  },

  // Get all tracked flights for a user
  getTrackedFlights: async (userId: string): Promise<TrackedFlight[]> => {
    const response = await api.get<TrackedFlight[]>('/tracking', {
      params: { userId },
    });
    return response.data;
  },

  // Get tracked flights grouped by route
  getTrackedFlightsByRoute: async (userId: string): Promise<{ routes: RouteGroup[] }> => {
    const response = await api.get('/tracking/by-route', {
      params: { userId },
    });
    return response.data;
  },

  // Get a specific tracked flight
  getTrackedFlight: async (id: string): Promise<TrackedFlight> => {
    const response = await api.get<TrackedFlight>(`/tracking/${id}`);
    return response.data;
  },

  // Update a tracked flight
  updateTrackedFlight: async (
    id: string,
    data: {
      notificationThresholdPercent?: number;
      pollingIntervalHours?: number;
      isActive?: boolean;
    }
  ): Promise<TrackedFlight> => {
    const response = await api.put<TrackedFlight>(`/tracking/${id}`, data);
    return response.data;
  },

  // Delete a tracked flight
  deleteTrackedFlight: async (id: string): Promise<void> => {
    await api.delete(`/tracking/${id}`);
  },

  // Get price history for a tracked flight
  getPriceHistory: async (id: string): Promise<PriceHistory[]> => {
    const response = await api.get<PriceHistory[]>(`/tracking/${id}/history`);
    return response.data;
  },

  // Search for flights
  searchFlights: async (data: SearchFlightsRequest): Promise<FlightSearchResult[]> => {
    const response = await api.post<FlightSearchResult[]>('/tracking/search', data);
    return response.data;
  },

  // Add recipient to tracked flight
  addRecipient: async (
    id: string,
    data: { email: string; name?: string }
  ): Promise<TrackedFlight> => {
    const response = await api.post<TrackedFlight>(`/tracking/${id}/recipients`, data);
    return response.data;
  },

  // Remove recipient from tracked flight
  removeRecipient: async (id: string, recipientId: string): Promise<void> => {
    await api.delete(`/tracking/${id}/recipients/${recipientId}`);
  },
};

// Error handling interceptor
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      // Server responded with error status
      const message = error.response.data?.message || error.response.data?.title || error.message;
      throw new Error(message);
    } else if (error.request) {
      // Request made but no response
      throw new Error('No response from server. Please check your connection.');
    } else {
      // Something else happened
      throw new Error(error.message);
    }
  }
);

export default api;
