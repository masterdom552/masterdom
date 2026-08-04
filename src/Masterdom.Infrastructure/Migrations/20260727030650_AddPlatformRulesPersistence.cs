using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformRulesPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_rule_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentRuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ScopeKind = table.Column<int>(type: "integer", nullable: false),
                    ScopeIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Compatibility = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InputKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ComparisonOperator = table.Column<int>(type: "integer", nullable: true),
                    CompareInputKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ExpectedValueKind = table.Column<int>(type: "integer", nullable: true),
                    ExpectedBoolean = table.Column<bool>(type: "boolean", nullable: true),
                    ExpectedNumber = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpectedText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MinNumber = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxNumber = table.Column<decimal>(type: "numeric", nullable: true),
                    CompositeOperator = table.Column<int>(type: "integer", nullable: true),
                    ArithmeticOperator = table.Column<int>(type: "integer", nullable: true),
                    ExpressionLeftKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ExpressionRightKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ExpressionExpectedNumber = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_rule_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "platform_rule_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ScopeKind = table.Column<int>(type: "integer", nullable: false),
                    ScopeIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Compatibility = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_rule_sets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_rule_definitions_ParentRuleId",
                table: "platform_rule_definitions",
                column: "ParentRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_rule_definitions_RuleSetId",
                table: "platform_rule_definitions",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_platform_rule_definitions_RuleSetId_Key_ScopeKind_ScopeIden~",
                table: "platform_rule_definitions",
                columns: new[] { "RuleSetId", "Key", "ScopeKind", "ScopeIdentifier", "Version", "EffectiveFromUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_rule_sets_Key_ScopeKind_ScopeIdentifier_Version_Ef~",
                table: "platform_rule_sets",
                columns: new[] { "Key", "ScopeKind", "ScopeIdentifier", "Version", "EffectiveFromUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_rule_definitions");

            migrationBuilder.DropTable(
                name: "platform_rule_sets");
        }
    }
}
