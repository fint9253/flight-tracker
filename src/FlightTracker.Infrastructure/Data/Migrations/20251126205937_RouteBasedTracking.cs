using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RouteBasedTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "TrackedFlights");

            migrationBuilder.AddColumn<int>(
                name: "DateFlexibilityDays",
                table: "TrackedFlights",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "MaxStops",
                table: "TrackedFlights",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateFlexibilityDays",
                table: "TrackedFlights");

            migrationBuilder.DropColumn(
                name: "MaxStops",
                table: "TrackedFlights");

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "TrackedFlights",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
