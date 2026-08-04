using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PolicyFrameworkFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "policies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    policy_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    policy_reference = table.Column<string>(type: "jsonb", nullable: false),
                    policy_scope = table.Column<string>(type: "jsonb", nullable: false),
                    policy_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expired_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "policy_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_scope = table.Column<string>(type: "jsonb", nullable: false),
                    assigned_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    assigned_entity_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    effective_date_range = table.Column<string>(type: "jsonb", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_policy_assignments_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    policy_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    effective_date_range = table.Column<string>(type: "jsonb", nullable: false),
                    policy_condition = table.Column<string>(type: "jsonb", nullable: false),
                    policy_metadata = table.Column<string>(type: "jsonb", nullable: false),
                    captured_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_policy_snapshots_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "policy_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    effective_date_range = table.Column<string>(type: "jsonb", nullable: false),
                    policy_condition = table.Column<string>(type: "jsonb", nullable: false),
                    policy_metadata = table.Column<string>(type: "jsonb", nullable: false),
                    policy_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expired_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_policy_versions_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_policies_policy_category",
                table: "policies",
                column: "policy_category");

            migrationBuilder.CreateIndex(
                name: "ix_policies_policy_status",
                table: "policies",
                column: "policy_status");

            migrationBuilder.CreateIndex(
                name: "ix_policies_policy_type",
                table: "policies",
                column: "policy_type");

            migrationBuilder.CreateIndex(
                name: "ix_policy_assignments_assignment_id",
                table: "policy_assignments",
                column: "assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_assignments_policy_id",
                table: "policy_assignments",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_snapshots_policy_id",
                table: "policy_snapshots",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_snapshots_snapshot_id",
                table: "policy_snapshots",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_versions_policy_id",
                table: "policy_versions",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "ix_policy_versions_version_number",
                table: "policy_versions",
                column: "version_number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_assignments");

            migrationBuilder.DropTable(
                name: "policy_snapshots");

            migrationBuilder.DropTable(
                name: "policy_versions");

            migrationBuilder.DropTable(
                name: "policies");
        }
    }
}
