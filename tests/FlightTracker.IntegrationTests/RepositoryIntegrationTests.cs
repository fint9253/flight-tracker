using FlightTracker.Core.Entities;
using FlightTracker.Infrastructure.Data;
using FlightTracker.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace FlightTracker.IntegrationTests;

public class RepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("flighttracker_test")
        .WithUsername("postgres")
        .WithPassword("test123")
        .Build();

    private FlightTrackerDbContext _context = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<FlightTrackerDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new FlightTrackerDbContext(options);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task TrackedFlightRepository_AddAndRetrieve_WorksCorrectly()
    {
        // Arrange
        var repository = new TrackedFlightRepository(_context);
        var flight = new TrackedFlight
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NotificationThresholdPercent = 5.00m,
            PollingIntervalMinutes = 15,
            IsActive = true
        };

        // Act
        var created = await repository.AddAsync(flight);
        var retrieved = await repository.GetByIdAsync(created.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.FlightNumber.Should().Be("AA123");
        retrieved.UserId.Should().Be("user123");
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task TrackedFlightRepository_GetByUserId_ReturnsUserFlights()
    {
        // Arrange
        var repository = new TrackedFlightRepository(_context);
        var userId = "user456";

        await repository.AddAsync(new TrackedFlight
        {
            UserId = userId,
            FlightNumber = "BA123",
            DepartureAirportIATA = "LHR",
            ArrivalAirportIATA = "JFK",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });

        await repository.AddAsync(new TrackedFlight
        {
            UserId = userId,
            FlightNumber = "BA456",
            DepartureAirportIATA = "LHR",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        });

        // Act
        var userFlights = await repository.GetByUserIdAsync(userId);

        // Assert
        userFlights.Should().HaveCount(2);
        userFlights.Should().AllSatisfy(f => f.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task TrackedFlightRepository_GetFlightsDueForPolling_ReturnsCorrectFlights()
    {
        // Arrange
        var repository = new TrackedFlightRepository(_context);

        // Flight that has never been polled
        await repository.AddAsync(new TrackedFlight
        {
            UserId = "user789",
            FlightNumber = "DL123",
            DepartureAirportIATA = "ATL",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PollingIntervalMinutes = 15,
            LastPolledAt = null
        });

        // Flight polled 20 minutes ago (due for polling with 15 min interval)
        await repository.AddAsync(new TrackedFlight
        {
            UserId = "user789",
            FlightNumber = "DL456",
            DepartureAirportIATA = "ATL",
            ArrivalAirportIATA = "JFK",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14)),
            PollingIntervalMinutes = 15,
            LastPolledAt = DateTime.UtcNow.AddMinutes(-20)
        });

        // Flight polled 5 minutes ago (not yet due)
        await repository.AddAsync(new TrackedFlight
        {
            UserId = "user789",
            FlightNumber = "DL789",
            DepartureAirportIATA = "ATL",
            ArrivalAirportIATA = "ORD",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(21)),
            PollingIntervalMinutes = 15,
            LastPolledAt = DateTime.UtcNow.AddMinutes(-5)
        });

        // Act
        var dueFlights = await repository.GetFlightsDueForPollingAsync(DateTime.UtcNow);

        // Assert
        dueFlights.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task PriceHistoryRepository_AddAndCalculateAverage_WorksCorrectly()
    {
        // Arrange
        var flightRepo = new TrackedFlightRepository(_context);
        var priceRepo = new PriceHistoryRepository(_context);

        var flight = await flightRepo.AddAsync(new TrackedFlight
        {
            UserId = "user123",
            FlightNumber = "AA999",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });

        // Add price history
        await priceRepo.AddAsync(new PriceHistory { TrackedFlightId = flight.Id, Price = 500m, Currency = "USD" });
        await priceRepo.AddAsync(new PriceHistory { TrackedFlightId = flight.Id, Price = 510m, Currency = "USD" });
        await priceRepo.AddAsync(new PriceHistory { TrackedFlightId = flight.Id, Price = 490m, Currency = "USD" });

        // Act
        var average = await priceRepo.GetAveragePriceAsync(flight.Id);

        // Assert
        average.Should().Be(500m); // (500 + 510 + 490) / 3 = 500
    }

    [Fact]
    public async Task PriceAlertRepository_GetUnprocessedAlerts_ReturnsOnlyUnprocessed()
    {
        // Arrange
        var flightRepo = new TrackedFlightRepository(_context);
        var alertRepo = new PriceAlertRepository(_context);

        var flight = await flightRepo.AddAsync(new TrackedFlight
        {
            UserId = "user555",
            FlightNumber = "UA123",
            DepartureAirportIATA = "ORD",
            ArrivalAirportIATA = "SFO",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });

        // Add alerts
        await alertRepo.AddAsync(new PriceAlert
        {
            TrackedFlightId = flight.Id,
            OldPrice = 500m,
            NewPrice = 450m,
            PercentageChange = -10m,
            Currency = "USD",
            IsProcessed = false
        });

        await alertRepo.AddAsync(new PriceAlert
        {
            TrackedFlightId = flight.Id,
            OldPrice = 450m,
            NewPrice = 400m,
            PercentageChange = -11.11m,
            Currency = "USD",
            IsProcessed = true // Already processed
        });

        // Act
        var unprocessed = await alertRepo.GetUnprocessedAlertsAsync();

        // Assert
        unprocessed.Should().ContainSingle();
        unprocessed.First().IsProcessed.Should().BeFalse();
        unprocessed.First().NewPrice.Should().Be(450m);
    }

    [Fact]
    public async Task NotificationRecipientRepository_AddAndRetrieve_WorksCorrectly()
    {
        // Arrange
        var flightRepo = new TrackedFlightRepository(_context);
        var recipientRepo = new NotificationRecipientRepository(_context);

        var flight = await flightRepo.AddAsync(new TrackedFlight
        {
            UserId = "user777",
            FlightNumber = "SW123",
            DepartureAirportIATA = "LAX",
            ArrivalAirportIATA = "LAS",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });

        await recipientRepo.AddAsync(new NotificationRecipient
        {
            TrackedFlightId = flight.Id,
            Email = "user1@example.com",
            Name = "User One",
            IsActive = true
        });

        await recipientRepo.AddAsync(new NotificationRecipient
        {
            TrackedFlightId = flight.Id,
            Email = "user2@example.com",
            Name = "User Two",
            IsActive = true
        });

        // Act
        var recipients = await recipientRepo.GetByFlightIdAsync(flight.Id);

        // Assert
        recipients.Should().HaveCount(2);
        recipients.Should().Contain(r => r.Email == "user1@example.com");
        recipients.Should().Contain(r => r.Email == "user2@example.com");
    }

    [Fact]
    public async Task FullIntegration_CreateFlightAddPricesCreateAlert_WorksEndToEnd()
    {
        // Arrange
        var flightRepo = new TrackedFlightRepository(_context);
        var priceRepo = new PriceHistoryRepository(_context);
        var alertRepo = new PriceAlertRepository(_context);
        var recipientRepo = new NotificationRecipientRepository(_context);

        // Act - Create tracked flight
        var flight = await flightRepo.AddAsync(new TrackedFlight
        {
            UserId = "integration_user",
            FlightNumber = "TEST123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            NotificationThresholdPercent = 5.00m,
            PollingIntervalMinutes = 15,
            IsActive = true
        });

        // Add recipients
        await recipientRepo.AddAsync(new NotificationRecipient
        {
            TrackedFlightId = flight.Id,
            Email = "test@example.com",
            IsActive = true
        });

        // Add price history to establish average
        await priceRepo.AddAsync(new PriceHistory { TrackedFlightId = flight.Id, Price = 500m, Currency = "USD" });
        await priceRepo.AddAsync(new PriceHistory { TrackedFlightId = flight.Id, Price = 510m, Currency = "USD" });
        await priceRepo.AddAsync(new PriceHistory { TrackedFlightId = flight.Id, Price = 490m, Currency = "USD" });

        // Calculate average and create alert
        var average = await priceRepo.GetAveragePriceAsync(flight.Id);
        var newPrice = 460m; // Below 5% threshold (475)
        var percentageChange = ((newPrice - average) / average) * 100;

        await alertRepo.AddAsync(new PriceAlert
        {
            TrackedFlightId = flight.Id,
            OldPrice = average,
            NewPrice = newPrice,
            PercentageChange = percentageChange,
            Currency = "USD",
            IsProcessed = false
        });

        // Assert - Verify full flow
        var retrievedFlight = await flightRepo.GetByIdAsync(flight.Id);
        retrievedFlight.Should().NotBeNull();

        var priceHistory = await priceRepo.GetByFlightIdAsync(flight.Id);
        priceHistory.Should().HaveCount(3);

        var unprocessedAlerts = await alertRepo.GetUnprocessedAlertsAsync();
        unprocessedAlerts.Should().Contain(a => a.TrackedFlightId == flight.Id);

        var recipients = await recipientRepo.GetActiveByFlightIdAsync(flight.Id);
        recipients.Should().ContainSingle();

        // Verify alert calculation
        average.Should().Be(500m);
        percentageChange.Should().BeApproximately(-8m, 0.1m);
    }

    [Fact]
    public async Task DatabaseMigrations_ApplySuccessfully()
    {
        // Assert - Verify tables were created
        var tables = await _context.Database.SqlQueryRaw<string>(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'")
            .ToListAsync();

        tables.Should().Contain("TrackedFlights");
        tables.Should().Contain("PriceHistories");
        tables.Should().Contain("PriceAlerts");
        tables.Should().Contain("NotificationRecipients");
    }
}
