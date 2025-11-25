import { test, expect } from '@playwright/test';

const TEST_USER_ID = 'test-user-price-history';
const BASE_URL = 'http://localhost:5000/api';

let testFlightId: string;

test.describe('Price History API', () => {

  test.describe.configure({ mode: 'serial' });

  test.beforeAll(async ({ request }) => {
    // Create a test flight
    const response = await request.post(`${BASE_URL}/tracking`, {
      data: {
        userId: TEST_USER_ID,
        flightNumber: 'LH400',
        departureAirportIATA: 'FRA',
        arrivalAirportIATA: 'JFK',
        departureDate: '2025-12-28',
        notificationThresholdPercent: 8,
        pollingIntervalMinutes: 20,
      },
    });

    const flight = await response.json();
    testFlightId = flight.id;
  });

  test('GET /api/tracking/{id}/price-history - Get price history (empty initially)', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking/${testFlightId}/price-history`);

    expect(response.ok()).toBeTruthy();
    const history = await response.json();
    expect(Array.isArray(history)).toBeTruthy();
    // Initially empty as no polling has occurred
    expect(history.length).toBe(0);
  });

  test('GET /api/tracking/{id}/price-history - 404 for non-existent flight', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking/00000000-0000-0000-0000-000000000000/price-history`);
    expect(response.status()).toBe(404);
  });

  test('GET /api/tracking/{id}/price-history - Validate response structure', async ({ request }) => {
    const response = await request.get(`${BASE_URL}/tracking/${testFlightId}/price-history`);

    expect(response.ok()).toBeTruthy();
    const history = await response.json();

    // Even if empty, should be valid array
    expect(Array.isArray(history)).toBeTruthy();

    // If there are price history entries, validate structure
    if (history.length > 0) {
      const entry = history[0];
      expect(entry).toHaveProperty('id');
      expect(entry).toHaveProperty('price');
      expect(entry).toHaveProperty('currency');
      expect(entry).toHaveProperty('pollTimestamp');
      expect(typeof entry.price).toBe('number');
      expect(entry.price).toBeGreaterThan(0);
    }
  });

  test.afterAll(async ({ request }) => {
    // Clean up test flight
    await request.delete(`${BASE_URL}/tracking/${testFlightId}`);
  });
});
