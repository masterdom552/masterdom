using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformMetadataPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "platform_metadata_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    ScopeKind = table.Column<int>(type: "integer", nullable: false),
                    ScopeIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByKey = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Compatibility = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_metadata_definitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_platform_metadata_definitions_Key_ScopeKind_ScopeIdentifier~",
                table: "platform_metadata_definitions",
                columns: new[] { "Key", "ScopeKind", "ScopeIdentifier", "Version", "EffectiveFromUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "platform_metadata_definitions");
        }
    }
}
