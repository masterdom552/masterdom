using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PropertyInvariantUniquenessConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_properties_Code",
                table: "properties",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_metadata_property_id_metadata_key",
                table: "property_metadata",
                columns: new[] { "property_id", "metadata_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_units_PropertyId_Code",
                table: "property_units",
                columns: new[] { "PropertyId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_properties_Code",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "IX_property_metadata_property_id_metadata_key",
                table: "property_metadata");

            migrationBuilder.DropIndex(
                name: "IX_property_units_PropertyId_Code",
                table: "property_units");
        }
    }
}
