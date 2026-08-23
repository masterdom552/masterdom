using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDelegatedAuthority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "DelegatedAuthority",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatedToUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DelegatedRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "jsonb", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RevocationReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Remarks = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegatedAuthority", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedAuthority_DelegatedRoleId",
                schema: "identity",
                table: "DelegatedAuthority",
                column: "DelegatedRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedAuthority_DelegatedToUserId",
                schema: "identity",
                table: "DelegatedAuthority",
                column: "DelegatedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedAuthority_DelegatedToUserId_EffectiveFromUtc_Effec~",
                schema: "identity",
                table: "DelegatedAuthority",
                columns: new[] { "DelegatedToUserId", "EffectiveFromUtc", "EffectiveToUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedAuthority_DelegatedToUserId_Status",
                schema: "identity",
                table: "DelegatedAuthority",
                columns: new[] { "DelegatedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedAuthority_DelegatorUserId",
                schema: "identity",
                table: "DelegatedAuthority",
                column: "DelegatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DelegatedAuthority_Status",
                schema: "identity",
                table: "DelegatedAuthority",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DelegatedAuthority",
                schema: "identity");
        }
    }
}
