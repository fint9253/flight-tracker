# Flight Tracker E2E Tests

This directory contains end-to-end API tests for the Flight Tracker application using Playwright.

## Test Structure

- `e2e/tracked-flights.api.spec.ts` - Tests for tracked flights CRUD operations
- `e2e/recipients.api.spec.ts` - Tests for recipient management
- `e2e/price-history.api.spec.ts` - Tests for price history retrieval

## Prerequisites

Before running tests, ensure:
1. PostgreSQL is running
2. Database migrations have been applied
3. .NET API server is running on `http://localhost:5000`

## Running Tests

### Install Dependencies
```bash
npm install
npx playwright install chromium
```

### Run All Tests
```bash
npm test
```

### Run API Tests Only
```bash
npm run test:api
```

### Run Tests in Headed Mode (with browser)
```bash
npm run test:headed
```

### Debug Tests
```bash
npm run test:debug
```

### View Test Report
```bash
npm run test:report
```

## Test Coverage

### Tracked Flights API (`/api/tracking`)
- ✅ Create tracked flight with validation
- ✅ Get all tracked flights for user
- ✅ Get specific tracked flight by ID
- ✅ Get flights grouped by route
- ✅ Update tracked flight settings
- ✅ Delete tracked flight
- ✅ Validation errors for invalid inputs
- ✅ 404 errors for non-existent resources

### Recipients API (`/api/tracking/{id}/recipients`)
- ✅ Add recipient to tracked flight
- ✅ Remove recipient from tracked flight
- ✅ Validate email format
- ✅ Prevent duplicate recipients
- ✅ 404 errors for non-existent resources

### Price History API (`/api/tracking/{id}/price-history`)
- ✅ Get price history for tracked flight
- ✅ Validate response structure
- ✅ 404 errors for non-existent flights

## CI/CD

Tests run automatically on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`

See `.github/workflows/playwright.yml` for CI configuration.

## Test Data

Tests use separate test user IDs to avoid conflicts:
- `test-user-playwright` - Tracked flights tests
- `test-user-recipients` - Recipients tests
- `test-user-price-history` - Price history tests

Tests clean up after themselves using `beforeAll` and `afterAll` hooks.
