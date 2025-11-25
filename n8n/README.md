# n8n Workflows for Flight Tracker

This directory contains n8n workflow automation for the Flight Tracker application.

## Overview

Two workflows are provided:
1. **Price Alert Notification** - Sends email when flight prices drop below threshold
2. **Daily Summary Email** - AI-generated daily summary of all tracked flights using Gemini

## Prerequisites

### 1. Start n8n
```bash
# Make sure .env file is configured (see below)
docker-compose up -d n8n postgres

# Access n8n at: http://localhost:5678
# Default credentials: admin/admin (change in .env)
```

### 2. Required API Keys & Credentials

#### Gmail OAuth2 (for sending emails)
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select existing
3. Enable **Gmail API**
4. Create **OAuth 2.0 Client ID** credentials:
   - Application type: Web application
   - Authorized redirect URIs: `http://localhost:5678/rest/oauth2-credential/callback`
5. Download credentials and save Client ID + Client Secret

#### Gemini API Key (for AI summaries)
1. Go to [Google AI Studio](https://ai.google.dev/)
2. Click "Get API Key"
3. Create new API key
4. **Free tier limits:** 15 requests/minute, 1500 requests/day

#### Environment Variables
Update your `.env` file:
```bash
# Gmail
GMAIL_ADDRESS=your.email@gmail.com

# Gemini AI
GEMINI_API_KEY=your_gemini_api_key_here

# n8n
N8N_USER=admin
N8N_PASSWORD=your_secure_password
```

## Setting Up Credentials in n8n

### Gmail OAuth2 Credentials

1. Open n8n: http://localhost:5678
2. Go to **Settings** → **Credentials**
3. Click **Add Credential**
4. Search for **Gmail OAuth2**
5. Enter:
   - **Client ID**: From Google Cloud Console
   - **Client Secret**: From Google Cloud Console
   - **Scopes**: `https://www.googleapis.com/auth/gmail.send`
6. Click **Connect my account**
7. Authorize access
8. **Save** with name: `Gmail OAuth2`

## Importing Workflows

### Method 1: Via n8n UI

1. Open n8n: http://localhost:5678
2. Click **Import Workflow** (top right)
3. Select workflow JSON file:
   - `workflows/price-alert-notification.json`
   - `workflows/daily-summary-email.json`
4. Click **Import**
5. Connect credentials:
   - Link **Gmail OAuth2** credential to email nodes
6. **Activate** workflow (toggle switch in top right)

### Method 2: Copy to n8n Data Directory

```bash
# Copy workflows to n8n's data directory
docker cp n8n/workflows/price-alert-notification.json flight-tracker-n8n:/home/node/.n8n/workflows/
docker cp n8n/workflows/daily-summary-email.json flight-tracker-n8n:/home/node/.n8n/workflows/

# Restart n8n
docker-compose restart n8n
```

## Workflow Details

### 1. Price Alert Notification

**Trigger:** Webhook (called by .NET API when price drops)

**Flow:**
1. Receive webhook with flight + price data
2. Format HTML email with:
   - Price drop amount & percentage
   - Flight details (number, route, date)
   - Previous vs current price
   - "Book Now" CTA link
3. Send email to all recipients

**Webhook URL:**
```
http://localhost:5678/webhook/price-alert
```

**Payload Example:**
```json
{
  "flightNumber": "EI101",
  "departureAirportIATA": "DUB",
  "arrivalAirportIATA": "MAD",
  "departureDate": "2025-12-25",
  "currentPrice": 89.99,
  "previousPrice": 120.00,
  "currency": "EUR",
  "recipients": [
    { "email": "user@example.com", "name": "John Doe" }
  ]
}
```

### 2. Daily Summary Email with Gemini AI

**Trigger:** Schedule (every day at 8 AM)

**Flow:**
1. Fetch all tracked flights from API
2. Group flights by active/inactive status
3. Send data to **Gemini AI** to generate friendly summary
4. Format HTML email with:
   - AI-generated greeting & summary
   - Statistics (total, active, inactive counts)
   - List of active flights with details
   - List of paused flights
   - "View Dashboard" CTA link
5. Send email to user

**Gemini AI Prompt:**
- Asks Gemini to write a friendly 2-3 sentence summary
- Includes flight counts and status
- Keeps tone warm and helpful
- Under 100 words

**Schedule:**
- Runs daily at 8:00 AM (configurable in workflow)
- Can be manually triggered for testing

## Testing Workflows

### Test Price Alert Workflow

1. Make sure workflow is **activated**
2. Get webhook URL from the workflow
3. Send test webhook:

```bash
curl -X POST http://localhost:5678/webhook/price-alert \
  -H "Content-Type: application/json" \
  -d '{
    "flightNumber": "TEST123",
    "departureAirportIATA": "DUB",
    "arrivalAirportIATA": "NYC",
    "departureDate": "2025-12-31",
    "currentPrice": 299.99,
    "previousPrice": 399.99,
    "currency": "EUR",
    "recipients": [
      {"email": "your.email@gmail.com", "name": "Test User"}
    ]
  }'
```

4. Check your email inbox

### Test Daily Summary Workflow

1. Make sure workflow is **activated**
2. Make sure API is running with some tracked flights
3. Click **Execute Workflow** button in n8n
4. Check execution log for any errors
5. Check your email inbox

**Troubleshooting:**
- Verify Gemini API key in environment
- Check API is accessible from n8n container
- Verify Gmail credentials are connected

## Integrating with .NET API

To trigger price alerts from your C# code:

```csharp
public class N8nNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl = "http://localhost:5678/webhook/price-alert";

    public async Task SendPriceAlertAsync(TrackedFlight flight, decimal currentPrice, decimal previousPrice)
    {
        var payload = new
        {
            flightNumber = flight.FlightNumber,
            departureAirportIATA = flight.DepartureAirportIATA,
            arrivalAirportIATA = flight.ArrivalAirportIATA,
            departureDate = flight.DepartureDate,
            currentPrice,
            previousPrice,
            currency = "EUR",
            recipients = flight.Recipients.Select(r => new { r.Email, r.Name })
        };

        await _httpClient.PostAsJsonAsync(_webhookUrl, payload);
    }
}
```

## Monitoring & Logs

### View Execution History
1. Open n8n: http://localhost:5678
2. Click **Executions** in left sidebar
3. See all workflow runs with:
   - Success/failure status
   - Execution time
   - Input/output data
   - Error messages

### Enable Debug Mode
In workflow editor:
1. Click node you want to debug
2. Click **Execute Node**
3. View output in right panel

## Customization

### Change Email Design
Edit the HTML in the **Format Email Content** / **Format Summary Email** nodes:
- Modify colors, fonts, layout
- Add your logo/branding
- Change CTA button text/link

### Adjust Schedule
In **Daily Summary** workflow:
- Click **Schedule Trigger** node
- Change time, frequency, timezone
- Save workflow

### Add More Notifications
Duplicate workflows and modify for:
- Slack notifications
- SMS via Twilio
- Push notifications
- Discord/Teams webhooks

## Costs

- **n8n**: Free (self-hosted)
- **Gemini API**: Free tier (15 req/min, 1500/day)
- **Gmail**: Free

**Estimated usage:**
- Price alerts: Variable (depends on price changes)
- Daily summary: 1 request/day = 30/month
- Well within free tier limits ✅

## Troubleshooting

### Workflow not triggering
- Check workflow is **activated** (toggle in top right)
- Verify webhook URL is correct
- Check n8n logs: `docker logs flight-tracker-n8n`

### Email not sending
- Verify Gmail OAuth2 credentials are connected
- Check Gmail API is enabled in Google Cloud
- Verify sender email matches Gmail account

### Gemini API errors
- Verify API key is correct in .env
- Check free tier limits not exceeded
- Ensure proper JSON format in request

### Can't reach API from n8n
- Use `host.docker.internal:5000` instead of `localhost:5000`
- Verify API is running and accessible
- Check docker network configuration

## Next Steps

1. ✅ Start n8n and PostgreSQL
2. ✅ Set up Gmail OAuth2 credentials
3. ✅ Get Gemini API key
4. ✅ Import both workflows
5. ✅ Test workflows manually
6. ✅ Integrate webhook calls in .NET API
7. ✅ Monitor executions and refine

Happy automating! ✈️
