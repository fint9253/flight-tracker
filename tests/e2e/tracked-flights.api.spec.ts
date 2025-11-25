import { test, expect } from '@playwright/test';

const TEST_USER_ID = 'test-user-playwright';
const BASE_URL = 'http://localhost:5000/api';

// Test data
const createFlightPayload = {
  userId: TEST_USER_ID,
  flightNumber: 'EI101',
  departureAirportIATA: 'DUB',
  arrivalAirportIATA: 'MAD',
  departureDate: '2025-12-25',
  notificationThresholdPercent: 10,
  pollingIntervalMinutes: 30,
};

let createdFlightId: string;

test.describe('Tracked Flights API', () => {

  test.describe.configure({ mode: 'serial' });

  test('POST /api/tracking - Create tracked flight', async ({ request }) => {
    const response = await request.post(`${BASE_URL}/tracking`, {
      data: createFlightPayload,
    });

    expect(response.ok()).toBeTruthy();
    expect(response.status()).toBe(200);

    const flight = await response.json();
    expect(flight).toHaveProperty('id');
    expect(flight.userId).toBe(TEST_USER_ID);
    expect(flight.flightNumber).toBe('EI101');
    expect(flight.departureAirportIATA).toBe('DUB');
    expect(flight.arrivalAirportIATA).toBe('MAD');
    expect(flight.notificationThresholdPercent).toBe(10);
    expect(flight.pollingIntervalMinutes).toBe(30);
    expect(flight.isActive).toBe(true);
    expect(flight.recipients).toEqual([]);

    // Save for subsequent tests
    createdFlightId = flight.id;
  });

  test('POST /api/tracking - Validation error for invalid flight number', async ({ request }) => {
    const response = await request.post(`${BASE_URL}/tracking`, {
      data: {
        ...createFlightPayload,
        flightNumber: 'INVALID',
      },
    });

    expect(response.status()).toBe(400);
    const error = await response.json();
    expect(error).toHaveProperty('errors');
  });

  test('POST /api/tracking - Validation error for invalid IATA code', async ({ request }) => {
    const response = await request.post(`${BASE_URL}/tracking`, {
      data: {
        ...createFlightPayload,
        departureAirportIATA: 'INVALID',
      },
    });

    expect(response.status()).toBe(400);
  });

  test('POST /api/tracking - Validation error for past date', async ({ request }) => {
    const response = await request.post(`${BASE_URL}/tracking`, {
      data: {
        ...createFlightPayload,
        departureDate: '2020-01-01',
      },
    });

    expect(response.status()).toBe(400);
  });

  test('GET /api/tracking - Get all tracked flights for user', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking`, {
      params: { userId: TEST_USER_ID },
    });

    expect(response.ok()).toBeTruthy();
    const flights = await response.json();
    expect(Array.isArray(flights)).toBeTruthy();
    expect(flights.length).toBeGreaterThan(0);

    const createdFlight = flights.find((f: any) => f.id === createdFlightId);
    expect(createdFlight).toBeDefined();
  });

  test('GET /api/tracking/{id} - Get specific tracked flight', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking/${createdFlightId}`);

    expect(response.ok()).toBeTruthy();
    const flight = await response.json();
    expect(flight.id).toBe(createdFlightId);
    expect(flight.flightNumber).toBe('EI101');
  });

  test('GET /api/tracking/{id} - 404 for non-existent flight', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking/00000000-0000-0000-0000-000000000000`);
    expect(response.status()).toBe(404);
  });

  test('GET /api/tracking/by-route - Get flights grouped by route', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking/by-route`, {
      params: { userId: TEST_USER_ID },
    });

    expect(response.ok()).toBeTruthy();
    const data = await response.json();
    expect(data).toHaveProperty('routes');
    expect(Array.isArray(data.routes)).toBeTruthy();

    if (data.routes.length > 0) {
      const route = data.routes[0];
      expect(route).toHaveProperty('route');
      expect(route).toHaveProperty('departureAirportIATA');
      expect(route).toHaveProperty('arrivalAirportIATA');
      expect(route).toHaveProperty('flightCount');
      expect(route).toHaveProperty('flights');
    }
  });

  test('PUT /api/tracking/{id} - Update tracked flight', async ({ request }) => {
    const response = await request.put(`${BASE_URL}/tracking/${createdFlightId}`, {
      data: {
        notificationThresholdPercent: 15,
        pollingIntervalMinutes: 60,
        isActive: false,
      },
    });

    expect(response.ok()).toBeTruthy();
    const flight = await response.json();
    expect(flight.notificationThresholdPercent).toBe(15);
    expect(flight.pollingIntervalMinutes).toBe(60);
    expect(flight.isActive).toBe(false);
  });

  test('PUT /api/tracking/{id} - Validation error for invalid threshold', async ({ request }) => {
    const response = await request.put(`${BASE_URL}/tracking/${createdFlightId}`, {
      data: {
        notificationThresholdPercent: 150, // > 100
      },
    });

    expect(response.status()).toBe(400);
  });

  test('DELETE /api/tracking/{id} - Delete tracked flight', async ({ request }) => {
    const response = await request.delete(`${BASE_URL}/tracking/${createdFlightId}`);
    expect(response.ok()).toBeTruthy();

    // Verify deletion
    const getResponse = await request.get(`${BASE_URL}/tracking/${createdFlightId}`);
    expect(getResponse.status()).toBe(404);
  });

  test('DELETE /api/tracking/{id} - 404 for non-existent flight', async ({ request }) => {
    const response = await request.delete(`${BASE_URL}/tracking/00000000-0000-0000-0000-000000000000`);
    expect(response.status()).toBe(404);
  });
});
