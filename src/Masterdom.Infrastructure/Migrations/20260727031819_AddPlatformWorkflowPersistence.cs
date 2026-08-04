using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformWorkflowPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_workflow_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    IsStart = table.Column<bool>(type: "boolean", nullable: false),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false),
                    RetryMaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    RetryDelayMilliseconds = table.Column<int>(type: "integer", nullable: false),
                    TimeoutMilliseconds = table.Column<int>(type: "integer", nullable: false),
                    CompensationOperation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_workflow_steps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_workflow_transitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchKind = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ConditionKind = table.Column<int>(type: "integer", nullable: false),
                    RuleSetKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RuleScopeKind = table.Column<int>(type: "integer", nullable: true),
                    RuleScopeIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_workflow_transitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_workflow_versions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Compatibility = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_workflow_versions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ScopeKind = table.Column<int>(type: "integer", nullable: false),
                    ScopeIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_workflows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_steps_WorkflowVersionId",
                table: "platform_workflow_steps",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_steps_WorkflowVersionId_Key",
                table: "platform_workflow_steps",
                columns: new[] { "WorkflowVersionId", "Key" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_transitions_FromStepId",
                table: "platform_workflow_transitions",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_transitions_WorkflowVersionId",
                table: "platform_workflow_transitions",
                column: "WorkflowVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_transitions_WorkflowVersionId_FromStepId_~",
                table: "platform_workflow_transitions",
                columns: new[] { "WorkflowVersionId", "FromStepId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_versions_WorkflowId",
                table: "platform_workflow_versions",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflow_versions_WorkflowId_Version_EffectiveFrom~",
                table: "platform_workflow_versions",
                columns: new[] { "WorkflowId", "Version", "EffectiveFromUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_workflows_Key_ScopeKind_ScopeIdentifier",
                table: "platform_workflows",
                columns: new[] { "Key", "ScopeKind", "ScopeIdentifier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_workflow_steps");

            migrationBuilder.DropTable(
                name: "platform_workflow_transitions");

            migrationBuilder.DropTable(
                name: "platform_workflow_versions");

            migrationBuilder.DropTable(
                name: "platform_workflows");
        }
    }
}
