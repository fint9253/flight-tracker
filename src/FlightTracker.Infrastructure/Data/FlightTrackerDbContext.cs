using FlightTracker.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightTracker.Infrastructure.Data;

public class FlightTrackerDbContext : DbContext
{
    public FlightTrackerDbContext(DbContextOptions<FlightTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<TrackedFlight> TrackedFlights => Set<TrackedFlight>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TrackedFlight configuration
        modelBuilder.Entity<TrackedFlight>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserId)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.DepartureAirportIATA)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.ArrivalAirportIATA)
                .IsRequired()
                .HasMaxLength(3);

            entity.Property(e => e.NotificationThresholdPercent)
                .HasPrecision(5, 2);

            // Indexes
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_TrackedFlights_UserId");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_TrackedFlights_IsActive");

            // Relationships
            entity.HasMany(e => e.PriceHistories)
                .WithOne(e => e.TrackedFlight)
                .HasForeignKey(e => e.TrackedFlightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.PriceAlerts)
                .WithOne(e => e.TrackedFlight)
                .HasForeignKey(e => e.TrackedFlightId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.NotificationRecipients)
                .WithOne(e => e.TrackedFlight)
                .HasForeignKey(e => e.TrackedFlightId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PriceHistory configuration
        modelBuilder.Entity<PriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Price)
                .HasPrecision(10, 2);

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            // Indexes
            entity.HasIndex(e => e.TrackedFlightId)
                .HasDatabaseName("IX_PriceHistory_TrackedFlightId");

            entity.HasIndex(e => e.PollTimestamp)
                .IsDescending()
                .HasDatabaseName("IX_PriceHistory_PollTimestamp");
        });

        // PriceAlert configuration
        modelBuilder.Entity<PriceAlert>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OldPrice)
                .HasPrecision(10, 2);

            entity.Property(e => e.NewPrice)
                .HasPrecision(10, 2);

            entity.Property(e => e.PercentageChange)
                .HasPrecision(5, 2);

            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);

            // Indexes
            entity.HasIndex(e => e.TrackedFlightId)
                .HasDatabaseName("IX_PriceAlerts_TrackedFlightId");

            entity.HasIndex(e => e.IsProcessed)
                .HasFilter("\"IsProcessed\" = false")
                .HasDatabaseName("IX_PriceAlerts_IsProcessed");
        });

        // NotificationRecipient configuration
        modelBuilder.Entity<NotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Name)
                .HasMaxLength(100);

            // Indexes
            entity.HasIndex(e => e.TrackedFlightId)
                .HasDatabaseName("IX_NotificationRecipients_TrackedFlightId");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_NotificationRecipients_Email");
        });
    }
}
