using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UtilityRatingEngineFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "utility_ratings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    meter_reference = table.Column<string>(type: "jsonb", nullable: false),
                    consumption_reference = table.Column<string>(type: "jsonb", nullable: false),
                    rating_period = table.Column<string>(type: "jsonb", nullable: false),
                    tariff_reference = table.Column<string>(type: "jsonb", nullable: false),
                    rated_units = table.Column<decimal>(type: "numeric", nullable: false),
                    rated_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    rating_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    rating_version = table.Column<int>(type: "integer", nullable: false),
                    rating_result = table.Column<string>(type: "jsonb", nullable: false),
                    rating_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    rated_consumption = table.Column<string>(type: "jsonb", nullable: false),
                    utility_rate = table.Column<string>(type: "jsonb", nullable: false),
                    tariff_schedule = table.Column<string>(type: "jsonb", nullable: false),
                    rated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utility_ratings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_utility_ratings_rated_at_utc",
                table: "utility_ratings",
                column: "rated_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "utility_ratings");
        }
    }
}
