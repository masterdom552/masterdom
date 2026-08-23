using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAuthorityLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No defaultValue is set deliberately (see ADR-0010 and the migration-safety
            // investigation recorded in the PKG-CAP-018 authority-level-fix implementation
            // report). No known database has existing Role rows -- the only database usage
            // found in the repository is a torn-down validation container and transient
            // EF InMemory test databases. If a database with pre-existing Role rows does
            // exist, this migration must fail loudly (a NOT NULL constraint violation)
            // rather than silently backfill an invalid/arbitrary authority level such as 0
            // (not a valid AuthorityLevels value) or Tenant (an unverifiable guess).
            migrationBuilder.AddColumn<int>(
                name: "AuthorityLevel",
                table: "Roles",
                type: "integer",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorityLevel",
                table: "Roles");
        }
    }
}
