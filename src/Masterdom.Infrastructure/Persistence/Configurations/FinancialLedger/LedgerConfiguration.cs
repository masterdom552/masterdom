using System.Text.Json;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Infrastructure.Persistence.Configurations.FinancialLedger;

public sealed class LedgerConfiguration : IEntityTypeConfiguration<LedgerAggregate>
{
    public void Configure(EntityTypeBuilder<LedgerAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ledgers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(LedgerId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.LedgerCode)
            .HasColumnName("ledger_code")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.LedgerName)
            .HasColumnName("ledger_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.OwnsMany(x => x.Accounts, accountBuilder =>
        {
            accountBuilder.ToTable("ledger_accounts");
            accountBuilder.WithOwner().HasForeignKey("ledger_id");
            accountBuilder.Property<int>("id");
            accountBuilder.HasKey("id");

            accountBuilder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            accountBuilder.Property(x => x.AccountReference)
                .HasConversion(
                    value => JsonSerializer.Serialize(new AccountReferencePersistenceModel(value.AccountCode, value.AccountName), JsonSerializerOptions.Web),
                    json => DeserializeAccountReference(json))
                .HasColumnName("account_reference")
                .HasColumnType("jsonb")
                .IsRequired();

            accountBuilder.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(100)
                .IsRequired();

            accountBuilder.Property(x => x.OpenedAtUtc)
                .HasColumnName("opened_at_utc")
                .IsRequired();

            accountBuilder.HasIndex(x => x.AccountId)
                .HasDatabaseName("ix_ledger_accounts_account_id");
        });

        builder.OwnsMany(x => x.Transactions, transactionBuilder =>
        {
            transactionBuilder.ToTable("ledger_transactions");
            transactionBuilder.WithOwner().HasForeignKey("ledger_id");
            transactionBuilder.Property<int>("id");
            transactionBuilder.HasKey("id");

            transactionBuilder.Property(x => x.TransactionId)
                .HasColumnName("transaction_id")
                .IsRequired();

            transactionBuilder.Property(x => x.PostingReference)
                .HasConversion(
                    value => value.Value,
                    value => PostingReference.Create(value))
                .HasColumnName("posting_reference")
                .HasMaxLength(200)
                .IsRequired();

            transactionBuilder.Property(x => x.SourceModule)
                .HasColumnName("source_module")
                .HasMaxLength(100)
                .IsRequired();

            transactionBuilder.Property(x => x.PostingDate)
                .HasConversion(
                    value => value.Value,
                    value => PostingDate.Create(value))
                .HasColumnName("posting_date")
                .IsRequired();

            transactionBuilder.Property(x => x.JournalNumber)
                .HasColumnName("journal_number")
                .HasMaxLength(200)
                .IsRequired();

            transactionBuilder.Property(x => x.Description)
                .HasColumnName("description")
                .HasMaxLength(1000)
                .IsRequired();

            transactionBuilder.Property(x => x.PostingStatus)
                .HasValueObjectConversion(PostingStatus.Create)
                .HasColumnName("posting_status")
                .HasMaxLength(50)
                .IsRequired();

            transactionBuilder.Property(x => x.IsReversal)
                .HasColumnName("is_reversal")
                .IsRequired();

            transactionBuilder.Property(x => x.ReversedTransactionId)
                .HasColumnName("reversed_transaction_id");

            transactionBuilder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            transactionBuilder.OwnsMany(x => x.JournalEntries, entryBuilder =>
            {
                entryBuilder.ToTable("journal_entries");
                entryBuilder.WithOwner().HasForeignKey("ledger_transaction_id");
                entryBuilder.Property<int>("id");
                entryBuilder.HasKey("id");

                entryBuilder.Property(x => x.EntryId)
                    .HasColumnName("entry_id")
                    .IsRequired();

                entryBuilder.Property(x => x.AccountReference)
                    .HasConversion(
                        value => JsonSerializer.Serialize(new AccountReferencePersistenceModel(value.AccountCode, value.AccountName), JsonSerializerOptions.Web),
                        json => DeserializeAccountReference(json))
                    .HasColumnName("account_reference")
                    .HasColumnType("jsonb")
                    .IsRequired();

                entryBuilder.Property(x => x.DebitAmount)
                    .HasConversion(
                        value => value.Value,
                        value => MoneyAmount.Create(value))
                    .HasColumnName("debit_amount")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entryBuilder.Property(x => x.CreditAmount)
                    .HasConversion(
                        value => value.Value,
                        value => MoneyAmount.Create(value))
                    .HasColumnName("credit_amount")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                entryBuilder.Property(x => x.Description)
                    .HasColumnName("description")
                    .HasMaxLength(1000)
                    .IsRequired();

                entryBuilder.HasIndex(x => x.EntryId)
                    .HasDatabaseName("ix_journal_entries_entry_id");
            });

            transactionBuilder.Navigation(x => x.JournalEntries)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            transactionBuilder.HasIndex(x => x.TransactionId)
                .HasDatabaseName("ix_ledger_transactions_transaction_id");

            transactionBuilder.HasIndex(x => x.PostingReference)
                .IsUnique()
                .HasDatabaseName("ux_ledger_transactions_posting_reference");

            transactionBuilder.HasIndex(x => x.JournalNumber)
                .IsUnique()
                .HasDatabaseName("ux_ledger_transactions_journal_number");
        });

        builder.OwnsMany(x => x.PostingBatches, batchBuilder =>
        {
            batchBuilder.ToTable("posting_batches");
            batchBuilder.WithOwner().HasForeignKey("ledger_id");
            batchBuilder.Property<int>("id");
            batchBuilder.HasKey("id");

            batchBuilder.Property(x => x.BatchId)
                .HasColumnName("batch_id")
                .IsRequired();

            batchBuilder.Property(x => x.BatchReference)
                .HasColumnName("batch_reference")
                .HasMaxLength(200)
                .IsRequired();

            batchBuilder.Property(x => x.SourceModule)
                .HasColumnName("source_module")
                .HasMaxLength(100)
                .IsRequired();

            batchBuilder.Property(x => x.PostingStatus)
                .HasValueObjectConversion(PostingStatus.Create)
                .HasColumnName("posting_status")
                .HasMaxLength(50)
                .IsRequired();

            batchBuilder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            batchBuilder.Property(x => x.CompletedAtUtc)
                .HasColumnName("completed_at_utc");

            batchBuilder.Property(x => x.TransactionIds)
                .HasConversion(
                    value => JsonSerializer.Serialize(value, JsonSerializerOptions.Web),
                    json => JsonSerializer.Deserialize<IReadOnlyList<Guid>>(json, JsonSerializerOptions.Web) ?? Array.Empty<Guid>())
                .HasColumnName("transaction_ids")
                .HasColumnType("jsonb")
                .IsRequired();

            batchBuilder.HasIndex(x => x.BatchId)
                .HasDatabaseName("ix_posting_batches_batch_id");
        });

        builder.OwnsMany(x => x.Snapshots, snapshotBuilder =>
        {
            snapshotBuilder.ToTable("ledger_snapshots");
            snapshotBuilder.WithOwner().HasForeignKey("ledger_id");
            snapshotBuilder.Property<int>("id");
            snapshotBuilder.HasKey("id");

            snapshotBuilder.Property(x => x.SnapshotId)
                .HasColumnName("snapshot_id")
                .IsRequired();

            snapshotBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            snapshotBuilder.Property(x => x.TransactionCount)
                .HasColumnName("transaction_count")
                .IsRequired();

            snapshotBuilder.Property(x => x.AccountCount)
                .HasColumnName("account_count")
                .IsRequired();

            snapshotBuilder.Property(x => x.TotalDebits)
                .HasConversion(
                    value => value.Value,
                    value => MoneyAmount.Create(value))
                .HasColumnName("total_debits")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            snapshotBuilder.Property(x => x.TotalCredits)
                .HasConversion(
                    value => value.Value,
                    value => MoneyAmount.Create(value))
                .HasColumnName("total_credits")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            snapshotBuilder.Property(x => x.CapturedAtUtc)
                .HasColumnName("captured_at_utc")
                .IsRequired();

            snapshotBuilder.HasIndex(x => x.SnapshotId)
                .HasDatabaseName("ix_ledger_snapshots_snapshot_id");
        });

        builder.OwnsMany(x => x.Versions, versionBuilder =>
        {
            versionBuilder.ToTable("ledger_versions");
            versionBuilder.WithOwner().HasForeignKey("ledger_id");
            versionBuilder.Property<int>("id");
            versionBuilder.HasKey("id");

            versionBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            versionBuilder.Property(x => x.ChangeReason)
                .HasColumnName("change_reason")
                .HasMaxLength(1000)
                .IsRequired();

            versionBuilder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            versionBuilder.HasIndex(x => x.VersionNumber)
                .HasDatabaseName("ix_ledger_versions_version_number");
        });

        builder.Navigation(x => x.Accounts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.PostingBatches).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Snapshots).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.CurrentVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.LedgerCode)
            .HasDatabaseName("ix_ledgers_ledger_code");
    }

    private static AccountReference DeserializeAccountReference(string json)
    {
        var model = JsonSerializer.Deserialize<AccountReferencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize account reference.");

        return AccountReference.Create(model.AccountCode, model.AccountName);
    }

    private sealed record AccountReferencePersistenceModel(string AccountCode, string AccountName);
}
