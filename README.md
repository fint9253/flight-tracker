# Flight Tracker

A flight price tracking application that monitors flight prices and sends notifications when prices drop below average.

## Architecture

This project uses **Clean Architecture with Vertical Slices (MediatR pattern)**.

### Structure

```
src/
├── FlightTracker.Core/              # Domain Layer
│   ├── Entities/                    # Domain models
│   └── Interfaces/                  # Repository contracts
│
├── FlightTracker.Infrastructure/    # Infrastructure Layer
│   ├── Data/                        # DbContext, EF Core
│   ├── Repositories/                # Repository implementations
│   └── Services/                    # External API clients (Amadeus)
│
└── FlightTracker.Api/               # Presentation Layer
    ├── Features/                    # Vertical slices with MediatR
    │   ├── CreateTrackedFlight/     # Each feature is self-contained
    │   │   ├── Command.cs           # Request DTO
    │   │   ├── Handler.cs           # Business logic
    │   │   └── Validator.cs         # FluentValidation
    │   └── ...
    └── Controllers/                 # Thin controllers (call MediatR)
```

### Key Design Decisions

**Clean Architecture Layers:**
- **Core**: Domain entities and repository interfaces (no dependencies)
- **Infrastructure**: Data access, external services (depends on Core)
- **Api**: Controllers and features (depends on Infrastructure)

**Vertical Slices with MediatR:**
- Features organized by use case, not technical layer
- Each feature contains its own DTOs, validation, and business logic
- MediatR handles request/response pipeline
- No shared DTOs across features (prevents coupling)

**Price Alert Logic:**
- Alerts trigger when price is below average by configurable threshold
- Each flight tracks its own price history
- n8n polls `PriceAlerts` table and sends notifications via Slack/Email

## Tech Stack

- **.NET 8** - Web API
- **PostgreSQL** - Database
- **Entity Framework Core** - ORM with migrations
- **MediatR** - CQRS pattern
- **FluentValidation** - Request validation
- **n8n** - Notification workflows
- **Docker Compose** - Local development environment

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop

### Running Locally

1. **Start infrastructure:**
   ```bash
   docker compose up -d
   ```

2. **Run migrations:**
   ```bash
   dotnet ef database update --project src/FlightTracker.Infrastructure --startup-project src/FlightTracker.Api
   ```

3. **Run API:**
   ```bash
   dotnet run --project src/FlightTracker.Api
   ```

4. **Access:**
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger
   - n8n: http://localhost:5678

## Configuration

Copy `.env.example` to `.env` and configure:
- `POSTGRES_PASSWORD` - Database password
- `N8N_PASSWORD` - n8n admin password
- `AMADEUS_API_KEY` - Flight data API key
- `GMAIL_APP_PASSWORD` - Email notifications
- `SLACK_BOT_TOKEN` - Slack notifications

## Database Schema

- **TrackedFlights** - User's flight tracking requests
- **PriceHistory** - Historical price data (for average calculation)
- **PriceAlerts** - Price drop notifications (polled by n8n)
- **NotificationRecipients** - Email addresses to notify per flight

## Development

See [GitHub Issues](https://github.com/fint9253/flight-tracker/issues) for current development tasks.
