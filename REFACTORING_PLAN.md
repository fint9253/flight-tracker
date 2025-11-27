# Flight Tracker: Route-Based Tracking Refactoring

## Overview
Refactoring the flight tracker from **flight-number-based tracking** to **route-based tracking** with flexible dates and stop preferences (like Skyscanner).

## Current Status: 🟡 IN PROGRESS

### ✅ Completed Changes

1. **Core Entity Updated** (`TrackedFlight.cs`)
   - ❌ Removed: `FlightNumber` field
   - ✅ Added: `DateFlexibilityDays` (default: 3 days)
   - ✅ Added: `MaxStops` (nullable: null=any, 0=direct, 1=1 stop, etc.)

2. **Database Context Updated** (`FlightTrackerDbContext.cs`)
   - Removed FlightNumber configuration

3. **Service Interface Updated** (`IFlightPriceService.cs`)
   - Added `GetRoutePriceAsync()` method with route-based parameters

4. **Price Polling Service Updated** (`PricePollingService.cs`)
   - Updated all logging to use route info instead of flight numbers
   - Changed to call `GetRoutePriceAsync()` with route parameters

5. **Amadeus Client Updated** (`AmadeusApiClient.cs`)
   - Added stub implementation of `GetRoutePriceAsync()` (needs full implementation)
   - Updated Amadeus API keys in configuration

---

## 🔴 Remaining Work

### 1. Database Migration
**Status:** Not created yet (code doesn't compile)
- Need to create EF Core migration for schema changes:
  - Remove `FlightNumber` column
  - Add `DateFlexibilityDays` column (int, default 3)
  - Add `MaxStops` column (int, nullable)

### 2. API Handlers - Fix FlightNumber References
**Files to update:**
- `Features/CreateTrackedFlight/Handler.cs` (3 errors)
- `Features/UpdateTrackedFlight/Handler.cs` (1 error)
- `Features/GetTrackedFlight/Handler.cs` (1 error)
- `Features/GetTrackedFlights/Handler.cs` (1 error)
- `Features/BatchCreateTrackedFlights/Handler.cs` (2 errors)
- `Features/GetTrackedFlightsByRoute/Handler.cs` (multiple errors)

**Changes needed:**
- Remove FlightNumber from request/response DTOs
- Add DateFlexibilityDays and MaxStops to DTOs
- Update validation logic
- Update mapping logic

### 3. Amadeus Service - Full Implementation
**File:** `AmadeusApiClient.cs`

**Implement `GetRoutePriceAsync()`:**
- Search for flights across date range (±N days)
- Filter by max stops
- Find cheapest option
- Handle multiple API calls if needed
- Proper caching strategy

**Amadeus API endpoints to use:**
- `/v2/shopping/flight-offers` with date range
- Filter results by number of stops
- Return lowest price

### 4. Frontend - Update Tracking Form
**File:** `client/src/components/FlightTrackingForm.tsx`

**Changes:**
- ❌ Remove: Flight Number input field
- ✅ Keep: Departure Airport, Arrival Airport
- ✅ Update: Date picker (keep single date, ±3 days is backend logic)
- ✅ Add: Stops dropdown (Direct, 1 Stop, 2+ Stops, Any)
- Update form validation
- Update API call payload

### 5. Frontend - Update Flight Display
**Files:**
- `client/src/components/TrackedFlightsList.tsx`
- `client/src/components/TrackedFlightCard.tsx`

**Changes:**
- Remove flight number display
- Show route info: "YVR → GUA"
- Show stops preference: "Direct flights only"
- Show date flexibility: "Dec 15 ±3 days"

### 6. n8n Workflows - Update
**Files:**
- `n8n/workflows/price-alert-notification.json`
- `n8n/workflows/daily-summary-email.json`

**Changes:**
- Remove FlightNumber from email templates
- Update to show route info instead
- Update webhook payload expectations

### 7. Testing
- Unit tests for route-based search
- Integration tests with Amadeus API
- E2E tests for tracking form
- Test date flexibility logic
- Test stops filtering

---

## Implementation Order (Recommended)

1. **Fix API Handlers** (to get code compiling)
   - Update all DTOs
   - Remove FlightNumber references
   - Add new fields

2. **Create Database Migration**
   - Run `dotnet ef migrations add RouteBasedTracking`
   - Apply migration to database

3. **Implement Amadeus Service**
   - Full implementation of GetRoutePriceAsync
   - Date range search
   - Stops filtering
   - Testing with real API

4. **Update Frontend**
   - Remove flight number field
   - Add stops dropdown
   - Update display components

5. **Update n8n Workflows**
   - Fix email templates
   - Test webhooks

6. **End-to-End Testing**
   - Test full flow
   - Verify price tracking works
   - Verify alerts trigger correctly

---

## Architecture Notes

### Old Model (Flight-Specific)
```
User tracks: AA123 (specific flight)
Date: 2025-12-15 (exact date)
System polls: Price for AA123 on that date
```

### New Model (Route-Based)
```
User tracks: YVR → GUA (route)
Date: 2025-12-15 ±3 days (flexible)
Stops: Direct only (preference)
System polls: Best price for that route within date range and stop constraints
```

### Key Differences
1. **No specific flight number** - tracks routes
2. **Date flexibility** - searches across multiple dates
3. **Stops filter** - user can prefer direct flights or allow connections
4. **Cheapest option** - always finds the best deal matching criteria

---

## API Configuration

### Amadeus API (Free Tier)
- **API Key:** `BPuUTfnh2eA3kJWolF8sxRm3QAPgKGns`
- **API Secret:** `ADqH7UTGTGXe3TLw`
- **Limits:** 2,000 calls/month
- **Endpoint:** `https://api.amadeus.com/v2/shopping/flight-offers`

---

## Questions / Decisions Needed

1. **Date Range Display:** Show exact dates user is tracking or just "±3 days"?
2. **Multiple Results:** Show multiple price points or just cheapest?
3. **Historical Tracking:** How to handle price history with flexible dates?
4. **Alert Logic:** Alert when price drops below average across all date options?

---

## Estimated Effort

- **API Handlers Update:** 2-3 hours
- **Database Migration:** 30 minutes
- **Amadeus Implementation:** 3-4 hours
- **Frontend Updates:** 2-3 hours
- **n8n Workflows:** 1 hour
- **Testing:** 2-3 hours

**Total:** ~12-15 hours

---

## Related Files Modified

### Backend
- `src/FlightTracker.Core/Entities/TrackedFlight.cs`
- `src/FlightTracker.Core/Interfaces/IFlightPriceService.cs`
- `src/FlightTracker.Infrastructure/Data/FlightTrackerDbContext.cs`
- `src/FlightTracker.Infrastructure/Services/PricePollingService.cs`
- `src/FlightTracker.Infrastructure/Services/AmadeusApiClient.cs`
- `src/FlightTracker.Api/appsettings.json`
- All files in `src/FlightTracker.Api/Features/**/Handler.cs`

### Frontend
- `client/src/components/FlightTrackingForm.tsx`
- `client/src/components/TrackedFlightsList.tsx`
- `client/src/components/TrackedFlightCard.tsx`

### Workflows
- `n8n/workflows/price-alert-notification.json`
- `n8n/workflows/daily-summary-email.json`

---

Generated: 2025-11-26
