using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SubsidyOptimizationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subsidy_optimization_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario = table.Column<string>(type: "jsonb", nullable: false),
                    meter_group = table.Column<string>(type: "jsonb", nullable: false),
                    rating_reference = table.Column<string>(type: "jsonb", nullable: false),
                    optimization_period = table.Column<string>(type: "jsonb", nullable: false),
                    optimization_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    optimization_version = table.Column<int>(type: "integer", nullable: false),
                    optimization_result = table.Column<string>(type: "jsonb", nullable: true),
                    consumption_forecast = table.Column<string>(type: "jsonb", nullable: true),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subsidy_optimization_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "optimization_recommendations",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recommendation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    details = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    archived_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    optimization_run_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimization_recommendations", x => x.id);
                    table.ForeignKey(
                        name: "FK_optimization_recommendations_subsidy_optimization_runs_opti~",
                        column: x => x.optimization_run_id,
                        principalTable: "subsidy_optimization_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "optimization_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    captured_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    optimization_result = table.Column<string>(type: "jsonb", nullable: false),
                    consumption_forecast = table.Column<string>(type: "jsonb", nullable: false),
                    recommendation_set = table.Column<string>(type: "jsonb", nullable: false),
                    optimization_run_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimization_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_optimization_snapshots_subsidy_optimization_runs_optimizati~",
                        column: x => x.optimization_run_id,
                        principalTable: "subsidy_optimization_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "optimization_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    optimization_run_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_optimization_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_optimization_versions_subsidy_optimization_runs_optimizatio~",
                        column: x => x.optimization_run_id,
                        principalTable: "subsidy_optimization_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_optimization_recommendations_optimization_run_id",
                table: "optimization_recommendations",
                column: "optimization_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_optimization_recommendations_recommendation_id",
                table: "optimization_recommendations",
                column: "recommendation_id");

            migrationBuilder.CreateIndex(
                name: "IX_optimization_snapshots_optimization_run_id",
                table: "optimization_snapshots",
                column: "optimization_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_optimization_snapshots_snapshot_id",
                table: "optimization_snapshots",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_optimization_versions_optimization_run_id",
                table: "optimization_versions",
                column: "optimization_run_id");

            migrationBuilder.CreateIndex(
                name: "ix_optimization_versions_version",
                table: "optimization_versions",
                column: "version");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "optimization_recommendations");

            migrationBuilder.DropTable(
                name: "optimization_snapshots");

            migrationBuilder.DropTable(
                name: "optimization_versions");

            migrationBuilder.DropTable(
                name: "subsidy_optimization_runs");
        }
    }
}
