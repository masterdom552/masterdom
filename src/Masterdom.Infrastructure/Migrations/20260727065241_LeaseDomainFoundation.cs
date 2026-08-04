using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LeaseDomainFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "leases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    lease_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    lease_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tenancy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    termination_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "lease_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    effective_period = table.Column<string>(type: "jsonb", nullable: false),
                    renewal_date = table.Column<DateOnly>(type: "date", nullable: true),
                    commercial_terms = table.Column<string>(type: "jsonb", nullable: false),
                    lease_clauses = table.Column<string>(type: "jsonb", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    lease_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lease_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_lease_versions_leases_lease_id",
                        column: x => x.lease_id,
                        principalTable: "leases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lease_versions_lease_id",
                table: "lease_versions",
                column: "lease_id");

            migrationBuilder.CreateIndex(
                name: "ix_lease_versions_version_active",
                table: "lease_versions",
                columns: new[] { "version_number", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_leases_lease_number",
                table: "leases",
                column: "lease_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lease_versions");

            migrationBuilder.DropTable(
                name: "leases");
        }
    }
}
