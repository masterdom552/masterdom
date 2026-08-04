using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TenancyDomainFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenancy_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    move_in_date = table.Column<DateOnly>(type: "date", nullable: false),
                    move_out_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    occupancy_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    closed_on = table.Column<DateOnly>(type: "date", nullable: true),
                    termination_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenancies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenancy_occupants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    tenancy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenancy_occupants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenancy_occupants_tenancies_tenancy_id",
                        column: x => x.tenancy_id,
                        principalTable: "tenancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenancies_tenancy_number",
                table: "tenancies",
                column: "tenancy_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenancy_occupants_person_primary",
                table: "tenancy_occupants",
                columns: new[] { "person_id", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "IX_tenancy_occupants_tenancy_id",
                table: "tenancy_occupants",
                column: "tenancy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tenancy_occupants");

            migrationBuilder.DropTable(
                name: "tenancies");
        }
    }
}
