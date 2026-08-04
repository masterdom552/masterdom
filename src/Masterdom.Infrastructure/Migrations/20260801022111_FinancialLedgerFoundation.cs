using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Masterdom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinancialLedgerFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ledgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ledger_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ledger_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledgers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ledger_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_reference = table.Column<string>(type: "jsonb", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    opened_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_accounts_ledgers_ledger_id",
                        column: x => x.ledger_id,
                        principalTable: "ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    transaction_count = table.Column<int>(type: "integer", nullable: false),
                    account_count = table.Column<int>(type: "integer", nullable: false),
                    total_debits = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_credits = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    captured_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_snapshots_ledgers_ledger_id",
                        column: x => x.ledger_id,
                        principalTable: "ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    posting_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    posting_date = table.Column<DateOnly>(type: "date", nullable: false),
                    journal_number = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    posting_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_reversal = table.Column<bool>(type: "boolean", nullable: false),
                    reversed_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_transactions_ledgers_ledger_id",
                        column: x => x.ledger_id,
                        principalTable: "ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    version_number = table.Column<int>(type: "integer", nullable: false),
                    change_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ledger_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_ledger_versions_ledgers_ledger_id",
                        column: x => x.ledger_id,
                        principalTable: "ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posting_batches",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_module = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    posting_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    transaction_ids = table.Column<string>(type: "jsonb", nullable: false),
                    ledger_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posting_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_posting_batches_ledgers_ledger_id",
                        column: x => x.ledger_id,
                        principalTable: "ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_reference = table.Column<string>(type: "jsonb", nullable: false),
                    debit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ledger_transaction_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_journal_entries_ledger_transactions_ledger_transaction_id",
                        column: x => x.ledger_transaction_id,
                        principalTable: "ledger_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_entry_id",
                table: "journal_entries",
                column: "entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_ledger_transaction_id",
                table: "journal_entries",
                column: "ledger_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_accounts_account_id",
                table: "ledger_accounts",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_accounts_ledger_id",
                table: "ledger_accounts",
                column: "ledger_id");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_snapshots_ledger_id",
                table: "ledger_snapshots",
                column: "ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_snapshots_snapshot_id",
                table: "ledger_snapshots",
                column: "snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_transactions_ledger_id",
                table: "ledger_transactions",
                column: "ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_transactions_transaction_id",
                table: "ledger_transactions",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_ledger_versions_ledger_id",
                table: "ledger_versions",
                column: "ledger_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_versions_version_number",
                table: "ledger_versions",
                column: "version_number");

            migrationBuilder.CreateIndex(
                name: "ix_ledgers_ledger_code",
                table: "ledgers",
                column: "ledger_code");

            migrationBuilder.CreateIndex(
                name: "ix_posting_batches_batch_id",
                table: "posting_batches",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_posting_batches_ledger_id",
                table: "posting_batches",
                column: "ledger_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "journal_entries");

            migrationBuilder.DropTable(
                name: "ledger_accounts");

            migrationBuilder.DropTable(
                name: "ledger_snapshots");

            migrationBuilder.DropTable(
                name: "ledger_versions");

            migrationBuilder.DropTable(
                name: "posting_batches");

            migrationBuilder.DropTable(
                name: "ledger_transactions");

            migrationBuilder.DropTable(
                name: "ledgers");
        }
    }
}
