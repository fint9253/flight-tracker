export interface TrackedFlight {
  id: string;
  userId: string;
  flightNumber: string;
  departureAirportIATA: string;
  arrivalAirportIATA: string;
  departureDate: string;
  notificationThresholdPercent: number;
  pollingIntervalMinutes: number;
  isActive: boolean;
  lastPolledAt: string | null;
  createdAt: string;
  updatedAt: string;
  recipients: NotificationRecipient[];
}

export interface NotificationRecipient {
  id: string;
  email: string;
  name: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface PriceHistory {
  id: string;
  price: number;
  currency: string;
  pollTimestamp: string;
}

export interface FlightSearchResult {
  flightNumber: string;
  airlineCode: string;
  originIATA: string;
  destinationIATA: string;
  departureDate: string;
  departureTime: string;
  arrivalTime: string;
  price: number;
  currency: string;
  numberOfStops: number;
  duration: string;
}

export interface CreateTrackedFlightRequest {
  userId: string;
  flightNumber: string;
  departureAirportIATA: string;
  arrivalAirportIATA: string;
  departureDate: string;
  notificationThresholdPercent?: number;
  pollingIntervalMinutes?: number;
}

export interface SearchFlightsRequest {
  originIATA: string;
  destinationIATA: string;
  departureDateStart: string;
  departureDateEnd: string;
  returnDateStart?: string;
  returnDateEnd?: string;
}

export interface RouteGroup {
  route: string;
  departureAirportIATA: string;
  arrivalAirportIATA: string;
  flightCount: number;
  activeFlightCount: number;
  inactiveFlightCount: number;
  earliestDepartureDate: string;
  latestDepartureDate: string;
  nextUpcomingFlight: RouteFlight | null;
  flights: RouteFlight[];
}

export interface RouteFlight {
  id: string;
  flightNumber: string;
  departureDate: string;
  notificationThresholdPercent: number;
  isActive: boolean;
  lastPolledAt: string | null;
}
