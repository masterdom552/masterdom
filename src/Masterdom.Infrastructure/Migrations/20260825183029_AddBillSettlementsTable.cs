using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillSettlementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bill_settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    allocation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    allocated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_reversed = table.Column<bool>(type: "boolean", nullable: false),
                    reversed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reversal_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bill_settlements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bill_settlements_allocation_id",
                table: "bill_settlements",
                column: "allocation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bill_settlements_bill_id",
                table: "bill_settlements",
                column: "bill_id");

            migrationBuilder.CreateIndex(
                name: "ix_bill_settlements_payment_id",
                table: "bill_settlements",
                column: "payment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bill_settlements");
        }
    }
}
