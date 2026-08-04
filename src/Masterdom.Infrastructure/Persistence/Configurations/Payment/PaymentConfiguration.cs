using System.Text.Json;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Infrastructure.Persistence.Configurations.Payment;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<PaymentAggregate>
{
    public void Configure(EntityTypeBuilder<PaymentAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(PaymentId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.PaymentReference)
            .HasConversion(
                value => value.Value,
                value => PaymentReference.Create(value))
            .HasColumnName("payment_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PaymentAmount)
            .HasConversion(
                value => value.Value,
                value => PaymentAmount.Create(value))
            .HasColumnName("payment_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.PaymentDate)
            .HasConversion(
                value => value.Value,
                value => PaymentDate.Create(value))
            .HasColumnName("payment_date")
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasValueObjectConversion(PaymentMethod.Create)
            .HasColumnName("payment_method")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PaymentStatus)
            .HasValueObjectConversion(PaymentStatus.Create)
            .HasColumnName("payment_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PaymentChannel)
            .HasValueObjectConversion(PaymentChannel.Create)
            .HasColumnName("payment_channel")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PaymentSource)
            .HasValueObjectConversion(PaymentSource.Create)
            .HasColumnName("payment_source")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReceivedAtUtc)
            .HasColumnName("received_at_utc")
            .IsRequired();

        builder.Property(x => x.ReversedAtUtc)
            .HasColumnName("reversed_at_utc");

        builder.Property(x => x.VoidedAtUtc)
            .HasColumnName("voided_at_utc");

        builder.Property(x => x.ReversalReason)
            .HasColumnName("reversal_reason")
            .HasMaxLength(1000);

        builder.Property(x => x.VoidReason)
            .HasColumnName("void_reason")
            .HasMaxLength(1000);

        builder.OwnsMany(x => x.Allocations, allocationBuilder =>
        {
            allocationBuilder.ToTable("payment_allocations");

            allocationBuilder.WithOwner()
                .HasForeignKey("payment_id");

            allocationBuilder.Property<int>("id");
            allocationBuilder.HasKey("id");

            allocationBuilder.Property(x => x.AllocationId)
                .HasColumnName("allocation_id")
                .IsRequired();

            allocationBuilder.Property(x => x.BillId)
                .HasColumnName("bill_id")
                .IsRequired();

            allocationBuilder.Property(x => x.BillNumber)
                .HasColumnName("bill_number")
                .HasMaxLength(200)
                .IsRequired();

            allocationBuilder.Property(x => x.Amount)
                .HasConversion(
                    value => value.Value,
                    value => PaymentAmount.Create(value))
                .HasColumnName("amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            allocationBuilder.Property(x => x.DueDate)
                .HasColumnName("due_date")
                .IsRequired();

            allocationBuilder.Property(x => x.AllocatedAtUtc)
                .HasColumnName("allocated_at_utc")
                .IsRequired();

            allocationBuilder.Property(x => x.IsReversed)
                .HasColumnName("is_reversed")
                .IsRequired();

            allocationBuilder.Property(x => x.ReversedAtUtc)
                .HasColumnName("reversed_at_utc");

            allocationBuilder.Property(x => x.ReversalReason)
                .HasColumnName("reversal_reason")
                .HasMaxLength(1000);

            allocationBuilder.HasIndex(x => x.AllocationId)
                .HasDatabaseName("ix_payment_allocations_allocation_id");
        });

        builder.OwnsMany(x => x.Versions, versionBuilder =>
        {
            versionBuilder.ToTable("payment_versions");

            versionBuilder.WithOwner()
                .HasForeignKey("payment_id");

            versionBuilder.Property<int>("id");
            versionBuilder.HasKey("id");

            versionBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            versionBuilder.Property(x => x.PaymentAmount)
                .HasConversion(
                    value => value.Value,
                    value => PaymentAmount.Create(value))
                .HasColumnName("payment_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            versionBuilder.Property(x => x.PaymentStatus)
                .HasValueObjectConversion(PaymentStatus.Create)
                .HasColumnName("payment_status")
                .HasMaxLength(50)
                .IsRequired();

            versionBuilder.Property(x => x.ChangeReason)
                .HasColumnName("change_reason")
                .HasMaxLength(1000)
                .IsRequired();

            versionBuilder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            versionBuilder.HasIndex(x => x.VersionNumber)
                .HasDatabaseName("ix_payment_versions_version_number");
        });

        builder.OwnsMany(x => x.Receipts, receiptBuilder =>
        {
            receiptBuilder.ToTable("payment_receipts");

            receiptBuilder.WithOwner()
                .HasForeignKey("payment_id");

            receiptBuilder.Property<int>("id");
            receiptBuilder.HasKey("id");

            receiptBuilder.Property(x => x.ReceiptId)
                .HasColumnName("receipt_id")
                .IsRequired();

            receiptBuilder.Property(x => x.ReceiptNumber)
                .HasColumnName("receipt_number")
                .HasMaxLength(200)
                .IsRequired();

            receiptBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            receiptBuilder.Property(x => x.Amount)
                .HasConversion(
                    value => value.Value,
                    value => PaymentAmount.Create(value))
                .HasColumnName("amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            receiptBuilder.Property(x => x.PaymentStatus)
                .HasValueObjectConversion(PaymentStatus.Create)
                .HasColumnName("payment_status")
                .HasMaxLength(50)
                .IsRequired();

            receiptBuilder.Property(x => x.IssuedAtUtc)
                .HasColumnName("issued_at_utc")
                .IsRequired();

            receiptBuilder.HasIndex(x => x.ReceiptId)
                .HasDatabaseName("ix_payment_receipts_receipt_id");
        });

        builder.OwnsMany(x => x.Snapshots, snapshotBuilder =>
        {
            snapshotBuilder.ToTable("payment_snapshots");

            snapshotBuilder.WithOwner()
                .HasForeignKey("payment_id");

            snapshotBuilder.Property<int>("id");
            snapshotBuilder.HasKey("id");

            snapshotBuilder.Property(x => x.SnapshotId)
                .HasColumnName("snapshot_id")
                .IsRequired();

            snapshotBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            snapshotBuilder.Property(x => x.PaymentStatus)
                .HasValueObjectConversion(PaymentStatus.Create)
                .HasColumnName("payment_status")
                .HasMaxLength(50)
                .IsRequired();

            snapshotBuilder.Property(x => x.PaymentAmount)
                .HasConversion(
                    value => value.Value,
                    value => PaymentAmount.Create(value))
                .HasColumnName("payment_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            snapshotBuilder.Property(x => x.AllocatedAmount)
                .HasConversion(
                    value => value.Value,
                    value => PaymentAmount.Create(value))
                .HasColumnName("allocated_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            snapshotBuilder.Property(x => x.UnallocatedAmount)
                .HasConversion(
                    value => value.Value,
                    value => PaymentAmount.Create(value))
                .HasColumnName("unallocated_amount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            snapshotBuilder.Property(x => x.Allocations)
                .HasConversion(
                    value => JsonSerializer.Serialize(value.Select(AllocationPersistenceModel.FromDomain).ToList(), JsonSerializerOptions.Web),
                    json => DeserializeAllocations(json))
                .HasColumnName("allocations")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.ReceiptNumber)
                .HasColumnName("receipt_number")
                .HasMaxLength(200)
                .IsRequired();

            snapshotBuilder.Property(x => x.CapturedAtUtc)
                .HasColumnName("captured_at_utc")
                .IsRequired();

            snapshotBuilder.HasIndex(x => x.SnapshotId)
                .HasDatabaseName("ix_payment_snapshots_snapshot_id");
        });

        builder.Navigation(x => x.Allocations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Versions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Receipts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Snapshots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.CurrentVersion);
        builder.Ignore(x => x.CurrentReceipt);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.PaymentReference)
            .HasDatabaseName("ix_payments_payment_reference");

        builder.HasIndex(x => x.PaymentStatus)
            .HasDatabaseName("ix_payments_payment_status");
    }

    private static IReadOnlyList<PaymentAllocation> DeserializeAllocations(string json)
    {
        var items = JsonSerializer.Deserialize<IReadOnlyList<AllocationPersistenceModel>>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize payment allocations snapshot.");

        return items.Select(x => x.ToDomain()).ToList();
    }

    private sealed record AllocationPersistenceModel(
        Guid BillId,
        string BillNumber,
        decimal Amount,
        DateOnly DueDate,
        DateTime AllocatedAtUtc,
        bool IsReversed,
        DateTime? ReversedAtUtc,
        string? ReversalReason)
    {
        public static AllocationPersistenceModel FromDomain(PaymentAllocation allocation)
        {
            return new AllocationPersistenceModel(
                allocation.BillId,
                allocation.BillNumber,
                allocation.Amount.Value,
                allocation.DueDate,
                allocation.AllocatedAtUtc,
                allocation.IsReversed,
                allocation.ReversedAtUtc,
                allocation.ReversalReason);
        }

        public PaymentAllocation ToDomain()
        {
            var allocation = PaymentAllocation.Create(
                BillId,
                BillNumber,
                PaymentAmount.Create(Amount),
                DueDate,
                AllocatedAtUtc);

            return IsReversed
                ? allocation.Reverse(ReversalReason ?? "Reversed", ReversedAtUtc ?? AllocatedAtUtc)
                : allocation;
        }
    }
}
