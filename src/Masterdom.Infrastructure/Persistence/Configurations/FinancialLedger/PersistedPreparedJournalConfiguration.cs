using Masterdom.Infrastructure.Persistence.FinancialLedger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.FinancialLedger;

public sealed class PersistedPreparedJournalConfiguration : IEntityTypeConfiguration<PersistedPreparedJournalEntity>
{
    public void Configure(EntityTypeBuilder<PersistedPreparedJournalEntity> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("prepared_journals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.LedgerId)
            .HasColumnName("ledger_id")
            .IsRequired();

        builder.Property(x => x.PostingReference)
            .HasColumnName("posting_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.JournalReference)
            .HasColumnName("journal_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.JournalNumber)
            .HasColumnName("journal_number")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PostingDate)
            .HasColumnName("posting_date")
            .IsRequired();

        builder.Property(x => x.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.BatchReference)
            .HasColumnName("batch_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SourceModule)
            .HasColumnName("source_module")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.Property(x => x.BillNumber)
            .HasColumnName("bill_number")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.DebitTotal)
            .HasColumnName("debit_total")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.CreditTotal)
            .HasColumnName("credit_total")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.State)
            .HasColumnName("state")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.ValidatedAtUtc)
            .HasColumnName("validated_at_utc");

        builder.Property(x => x.PostedAtUtc)
            .HasColumnName("posted_at_utc");

        builder.Property(x => x.ReversedAtUtc)
            .HasColumnName("reversed_at_utc");

        builder.Property(x => x.CancelledAtUtc)
            .HasColumnName("cancelled_at_utc");

        builder.Property(x => x.CancellationReason)
            .HasColumnName("cancellation_reason")
            .HasMaxLength(500);

        builder.Property(x => x.LedgerTransactionId)
            .HasColumnName("ledger_transaction_id");

        builder.Property(x => x.LinesJson)
            .HasColumnName("lines_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.MetadataJson)
            .HasColumnName("metadata_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(x => new { x.LedgerId, x.PostingReference })
            .IsUnique()
            .HasDatabaseName("ux_prepared_journals_ledger_posting_reference");

        builder.HasIndex(x => new { x.LedgerId, x.JournalNumber })
            .IsUnique()
            .HasDatabaseName("ux_prepared_journals_ledger_journal_number");

        builder.HasIndex(x => x.LedgerTransactionId)
            .HasDatabaseName("ix_prepared_journals_ledger_transaction_id");
    }
}
