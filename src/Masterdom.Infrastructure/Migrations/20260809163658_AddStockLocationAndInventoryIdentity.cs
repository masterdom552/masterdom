using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockLocationAndInventoryIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_property_units_PropertyId",
                table: "property_units");

            migrationBuilder.DropIndex(
                name: "IX_property_metadata_property_id",
                table: "property_metadata");

            migrationBuilder.CreateTable(
                name: "crm_parties",
                columns: table => new
                {
                    party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    legal_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    party_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_parties", x => x.party_id);
                });

            // Handles both fresh databases (CREATE) and legacy databases (ADD COLUMN).
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS inventory_items (
                    ""Id"" uuid NOT NULL,
                    property_id uuid NOT NULL,
                    stock_location_id uuid NULL,
                    sku character varying(64) NOT NULL,
                    name character varying(200) NOT NULL,
                    quantity_on_hand numeric(18,2) NOT NULL,
                    created_at_utc timestamptz NOT NULL,
                    CONSTRAINT ""PK_inventory_items"" PRIMARY KEY (""Id"")
                );
                ALTER TABLE inventory_items ADD COLUMN IF NOT EXISTS stock_location_id UUID;
            ");

            migrationBuilder.CreateTable(
                name: "maintenance_tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    assigned_to_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stock_locations",
                columns: table => new
                {
                    stock_location_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    property_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_locations", x => x.stock_location_id);
                    table.ForeignKey(
                        name: "FK_stock_locations_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "crm_party_addresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    state_or_province = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    country = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_party_addresses", x => x.id);
                    table.ForeignKey(
                        name: "FK_crm_party_addresses_crm_parties_party_id",
                        column: x => x.party_id,
                        principalTable: "crm_parties",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crm_party_contact_methods",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    contact_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    contact_value = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_party_contact_methods", x => x.id);
                    table.ForeignKey(
                        name: "FK_crm_party_contact_methods_crm_parties_party_id",
                        column: x => x.party_id,
                        principalTable: "crm_parties",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crm_party_relationships",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    related_party_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relationship_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    allows_self_reference = table.Column<bool>(type: "boolean", nullable: false),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_party_relationships", x => x.id);
                    table.ForeignKey(
                        name: "FK_crm_party_relationships_crm_parties_party_id",
                        column: x => x.party_id,
                        principalTable: "crm_parties",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "crm_party_role_assignments",
                columns: table => new
                {
                    party_role_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_from_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    assignment_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    deactivated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deactivation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    removed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    removal_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reactivated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reactivation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    party_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_party_role_assignments", x => x.party_role_assignment_id);
                    table.ForeignKey(
                        name: "FK_crm_party_role_assignments_crm_parties_party_id",
                        column: x => x.party_id,
                        principalTable: "crm_parties",
                        principalColumn: "party_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_property_units_PropertyId_Code",
                table: "property_units",
                columns: new[] { "PropertyId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_metadata_property_id_metadata_key",
                table: "property_metadata",
                columns: new[] { "property_id", "metadata_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_properties_Code",
                table: "properties",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crm_party_addresses_party_id",
                table: "crm_party_addresses",
                column: "party_id");

            migrationBuilder.CreateIndex(
                name: "IX_crm_party_contact_methods_party_id_contact_type_contact_val~",
                table: "crm_party_contact_methods",
                columns: new[] { "party_id", "contact_type", "contact_value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crm_party_relationships_party_id_related_party_id_relations~",
                table: "crm_party_relationships",
                columns: new[] { "party_id", "related_party_id", "relationship_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_crm_party_role_assignments_party_id_role_type_status",
                table: "crm_party_role_assignments",
                columns: new[] { "party_id", "role_type", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_crm_party_role_assignments_role_type_status",
                table: "crm_party_role_assignments",
                columns: new[] { "role_type", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_tickets_assigned_to_person_id",
                table: "maintenance_tickets",
                column: "assigned_to_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_tickets_property_id",
                table: "maintenance_tickets",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_tickets_status",
                table: "maintenance_tickets",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_tickets_unit_id",
                table: "maintenance_tickets",
                column: "unit_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_locations_property_id",
                table: "stock_locations",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "ux_stock_locations_property_id_name",
                table: "stock_locations",
                columns: new[] { "property_id", "name" },
                unique: true);

            // INV-2.3-M1: create one General StockLocation per property (deterministic, idempotent)
            migrationBuilder.Sql(@"
                INSERT INTO stock_locations (stock_location_id, property_id, name, is_active, code)
                SELECT gen_random_uuid(), property_id, 'General', true, 'GENERAL'
                FROM (SELECT DISTINCT property_id FROM inventory_items) AS dp
                WHERE NOT EXISTS (
                    SELECT 1 FROM stock_locations sl
                    WHERE sl.property_id = dp.property_id AND sl.name = 'General'
                )
            ");

            // INV-2.3-M1: assign each legacy InventoryItem to its property's General StockLocation
            migrationBuilder.Sql(@"
                UPDATE inventory_items ii
                SET stock_location_id = sl.stock_location_id
                FROM stock_locations sl
                WHERE sl.property_id = ii.property_id
                AND sl.name = 'General'
                AND ii.stock_location_id IS NULL
            ");

            // Enforce NOT NULL after all existing rows have a valid stock_location_id
            migrationBuilder.Sql("ALTER TABLE inventory_items ALTER COLUMN stock_location_id SET NOT NULL");

            // IF NOT EXISTS guards handle both fresh databases and legacy databases with existing indexes
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_inventory_items_property_id ON inventory_items (property_id)");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_inventory_items_stock_location_id ON inventory_items (stock_location_id)");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_inventory_items_property_id_sku");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS ux_inventory_items_property_id_stock_location_id_sku ON inventory_items (property_id, stock_location_id, sku)");

            migrationBuilder.AddForeignKey(
                name: "fk_inventory_items_stock_locations_stock_location_id",
                table: "inventory_items",
                column: "stock_location_id",
                principalTable: "stock_locations",
                principalColumn: "stock_location_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_inventory_items_stock_locations_stock_location_id",
                table: "inventory_items");

            migrationBuilder.DropTable(
                name: "crm_party_addresses");

            migrationBuilder.DropTable(
                name: "crm_party_contact_methods");

            migrationBuilder.DropTable(
                name: "crm_party_relationships");

            migrationBuilder.DropTable(
                name: "crm_party_role_assignments");

            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_inventory_items_property_id_stock_location_id_sku");
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_inventory_items_stock_location_id");
            migrationBuilder.Sql("CREATE UNIQUE INDEX IF NOT EXISTS ux_inventory_items_property_id_sku ON inventory_items (property_id, sku)");
            migrationBuilder.Sql("ALTER TABLE inventory_items DROP COLUMN IF EXISTS stock_location_id");

            migrationBuilder.DropTable(
                name: "maintenance_tickets");

            migrationBuilder.DropTable(
                name: "stock_locations");

            migrationBuilder.DropTable(
                name: "crm_parties");

            migrationBuilder.DropIndex(
                name: "IX_property_units_PropertyId_Code",
                table: "property_units");

            migrationBuilder.DropIndex(
                name: "IX_property_metadata_property_id_metadata_key",
                table: "property_metadata");

            migrationBuilder.DropIndex(
                name: "IX_properties_Code",
                table: "properties");

            migrationBuilder.CreateIndex(
                name: "IX_property_units_PropertyId",
                table: "property_units",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_property_metadata_property_id",
                table: "property_metadata",
                column: "property_id");
        }
    }
}
