using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightTracker.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferDetailsToPriceHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OfferDetailsJson",
                table: "PriceHistories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfferDetailsJson",
                table: "PriceHistories");
        }
    }
}
