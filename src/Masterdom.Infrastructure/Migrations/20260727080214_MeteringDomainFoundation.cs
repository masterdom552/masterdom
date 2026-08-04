using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MeteringDomainFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    meter_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meter_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meter_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meter_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    meter_location = table.Column<string>(type: "jsonb", nullable: false),
                    installation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    removal_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meter_readings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    reading_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reading_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reading_value = table.Column<decimal>(type: "numeric", nullable: false),
                    reading_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    submitted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reading_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    approval_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_rollover = table.Column<bool>(type: "boolean", nullable: false),
                    consumption = table.Column<decimal>(type: "numeric", nullable: true),
                    reviewed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    review_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reading_notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    correction_history = table.Column<string>(type: "jsonb", nullable: false),
                    reading_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    meter_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meter_readings", x => x.id);
                    table.ForeignKey(
                        name: "FK_meter_readings_meters_meter_id",
                        column: x => x.meter_id,
                        principalTable: "meters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_meter_reading_date",
                table: "meter_readings",
                columns: new[] { "meter_id", "reading_date" });

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_reading_id",
                table: "meter_readings",
                column: "reading_id");

            migrationBuilder.CreateIndex(
                name: "IX_meters_meter_number",
                table: "meters",
                column: "meter_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "meter_readings");

            migrationBuilder.DropTable(
                name: "meters");
        }
    }
}
