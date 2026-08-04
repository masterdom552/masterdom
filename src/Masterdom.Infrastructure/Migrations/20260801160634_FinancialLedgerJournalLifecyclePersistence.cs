using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinancialLedgerJournalLifecyclePersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prepared_journals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    journal_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    journal_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: false),
                    currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    batch_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bill_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    debit_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    state = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    validated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    posted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reversed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ledger_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lines_json = table.Column<string>(type: "jsonb", nullable: false),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prepared_journals", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_ledger_transactions_journal_number",
                table: "ledger_transactions",
                column: "journal_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_ledger_transactions_posting_reference",
                table: "ledger_transactions",
                column: "posting_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_prepared_journals_ledger_transaction_id",
                table: "prepared_journals",
                column: "ledger_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ux_prepared_journals_ledger_journal_number",
                table: "prepared_journals",
                columns: new[] { "ledger_id", "journal_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_prepared_journals_ledger_posting_reference",
                table: "prepared_journals",
                columns: new[] { "ledger_id", "posting_reference" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prepared_journals");

            migrationBuilder.DropIndex(
                name: "ux_ledger_transactions_journal_number",
                table: "ledger_transactions");

            migrationBuilder.DropIndex(
                name: "ux_ledger_transactions_posting_reference",
                table: "ledger_transactions");
        }
    }
}
