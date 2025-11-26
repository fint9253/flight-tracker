import { test, expect } from '@playwright/test';

const TEST_USER_ID = 'test-user-recipients';
const BASE_URL = 'http://localhost:5000/api';

let testFlightId: string;
let testRecipientId: string;

test.describe('Recipients API', () => {

  test.describe.configure({ mode: 'serial' });

  test.beforeAll(async ({ request }) => {
    // Create a test flight to work with
    const response = await request.post(`${BASE_URL}/tracking`, {
      data: {
        userId: TEST_USER_ID,
        departureAirportIATA: 'DUB',
        arrivalAirportIATA: 'BCN',
        departureDate: '2025-12-30',
        dateFlexibilityDays: 3,
        maxStops: 1,
        notificationThresholdPercent: 5,
        pollingIntervalMinutes: 15,
      },
    });

    const flight = await response.json();
    testFlightId = flight.id;
  });

  test('POST /api/tracking/{id}/recipients - Add recipient', async ({ request }) => {
    const response = await request.post(`${BASE_URL}/tracking/${testFlightId}/recipients`, {
      data: {
        email: 'test@example.com',
        name: 'Test User',
      },
    });

    expect(response.ok()).toBeTruthy();
    const recipient = await response.json();
    expect(recipient.email).toBe('test@example.com');
    expect(recipient.name).toBe('Test User');
    expect(recipient).toHaveProperty('id');

    testRecipientId = recipient.id;
  });

  test.skip('POST /api/tracking/{id}/recipients - Validation error for invalid email', async ({ request }) => {
    // TODO: Fix FluentValidation to return 400 instead of throwing 500 exception
    const response = await request.post(`${BASE_URL}/tracking/${testFlightId}/recipients`, {
      data: {
        email: 'invalid-email',
        name: 'Test User',
      },
    });

    expect(response.status()).toBe(400);
  });

  test('POST /api/tracking/{id}/recipients - Add second recipient', async ({ request }) => {
    const response = await request.post(`${BASE_URL}/tracking/${testFlightId}/recipients`, {
      data: {
        email: 'another@example.com',
      },
    });

    expect(response.ok()).toBeTruthy();
    const recipient = await response.json();
    expect(recipient.email).toBe('another@example.com');
    expect(recipient).toHaveProperty('id');
  });

  test.skip('POST /api/tracking/{id}/recipients - Prevent duplicate email', async ({ request }) => {
    // TODO: Add duplicate email validation
    const response = await request.post(`${BASE_URL}/tracking/${testFlightId}/recipients`, {
      data: {
        email: 'test@example.com', // Already exists
        name: 'Duplicate User',
      },
    });

    expect(response.status()).toBe(400);
    const error = await response.json();
    expect(error.title).toContain('already exists');
  });

  test('DELETE /api/tracking/{id}/recipients/{recipientId} - Remove recipient', async ({ request }) => {
    const response = await request.delete(`${BASE_URL}/tracking/${testFlightId}/recipients/${testRecipientId}`);
    expect(response.status()).toBe(204);
  });

  test('DELETE /api/tracking/{id}/recipients/{recipientId} - 404 for non-existent recipient', async ({ request }) => {
    const response = await request.delete(`${BASE_URL}/tracking/${testFlightId}/recipients/00000000-0000-0000-0000-000000000000`);
    expect(response.status()).toBe(404);
  });

  test.afterAll(async ({ request }) => {
    // Clean up test flight
    await request.delete(`${BASE_URL}/tracking/${testFlightId}`);
  });
});
