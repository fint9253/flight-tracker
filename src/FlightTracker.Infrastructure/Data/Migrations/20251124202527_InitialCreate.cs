using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackedFlights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FlightNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DepartureAirportIATA = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    ArrivalAirportIATA = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DepartureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NotificationThresholdPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PollingIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastPolledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedFlights", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackedFlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_TrackedFlights_TrackedFlightId",
                        column: x => x.TrackedFlightId,
                        principalTable: "TrackedFlights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackedFlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    NewPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PercentageChange = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    AlertTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceAlerts_TrackedFlights_TrackedFlightId",
                        column: x => x.TrackedFlightId,
                        principalTable: "TrackedFlights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrackedFlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    PollTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_TrackedFlights_TrackedFlightId",
                        column: x => x.TrackedFlightId,
                        principalTable: "TrackedFlights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_Email",
                table: "NotificationRecipients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_TrackedFlightId",
                table: "NotificationRecipients",
                column: "TrackedFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAlerts_IsProcessed",
                table: "PriceAlerts",
                column: "IsProcessed",
                filter: "\"IsProcessed\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PriceAlerts_TrackedFlightId",
                table: "PriceAlerts",
                column: "TrackedFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_PollTimestamp",
                table: "PriceHistories",
                column: "PollTimestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_TrackedFlightId",
                table: "PriceHistories",
                column: "TrackedFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedFlights_IsActive",
                table: "TrackedFlights",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedFlights_UserId",
                table: "TrackedFlights",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationRecipients");

            migrationBuilder.DropTable(
                name: "PriceAlerts");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "TrackedFlights");
        }
    }
}
