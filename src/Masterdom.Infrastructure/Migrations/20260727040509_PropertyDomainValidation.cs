using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PropertyDomainValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "capacity",
                table: "property_units",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_unit_id",
                table: "property_units",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_city",
                table: "properties",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_country_code",
                table: "properties",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line1",
                table: "properties",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_line2",
                table: "properties",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_postal_code",
                table: "properties",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_state_or_province",
                table: "properties",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "settings_allow_negative_occupancy",
                table: "properties",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "settings_currency_code",
                table: "properties",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "settings_time_zone_id",
                table: "properties",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "property_metadata",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    metadata_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    metadata_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_metadata", x => x.id);
                    table.ForeignKey(
                        name: "FK_property_metadata_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_relationships",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    target_property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_type = table.Column<int>(type: "integer", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_relationships", x => x.id);
                    table.ForeignKey(
                        name: "FK_property_relationships_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_property_metadata_property_id",
                table: "property_metadata",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_relationships_property_id",
                table: "property_relationships",
                column: "property_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "property_metadata");

            migrationBuilder.DropTable(
                name: "property_relationships");

            migrationBuilder.DropColumn(
                name: "capacity",
                table: "property_units");

            migrationBuilder.DropColumn(
                name: "parent_unit_id",
                table: "property_units");

            migrationBuilder.DropColumn(
                name: "address_city",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "address_country_code",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "address_line1",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "address_line2",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "address_postal_code",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "address_state_or_province",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "settings_allow_negative_occupancy",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "settings_currency_code",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "settings_time_zone_id",
                table: "properties");
        }
    }
}
