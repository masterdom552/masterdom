using Masterdom.Infrastructure.Persistence.Settlement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Settlement;

public sealed class BillSettlementConfiguration : IEntityTypeConfiguration<BillSettlement>
{
    public void Configure(EntityTypeBuilder<BillSettlement> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("bill_settlements");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.AllocationId)
            .HasColumnName("allocation_id")
            .IsRequired();

        builder.Property(x => x.BillId)
            .HasColumnName("bill_id")
            .IsRequired();

        builder.Property(x => x.BillNumber)
            .HasColumnName("bill_number")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .IsRequired();

        builder.Property(x => x.PaymentReference)
            .HasColumnName("payment_reference")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.AllocatedAtUtc)
            .HasColumnName("allocated_at_utc")
            .IsRequired();

        builder.Property(x => x.IsReversed)
            .HasColumnName("is_reversed")
            .IsRequired();

        builder.Property(x => x.ReversedAtUtc)
            .HasColumnName("reversed_at_utc");

        builder.Property(x => x.ReversalReason)
            .HasColumnName("reversal_reason")
            .HasMaxLength(1000);

        builder.HasIndex(x => x.AllocationId)
            .IsUnique()
            .HasDatabaseName("ix_bill_settlements_allocation_id");

        builder.HasIndex(x => x.BillId)
            .HasDatabaseName("ix_bill_settlements_bill_id");

        builder.HasIndex(x => x.PaymentId)
            .HasDatabaseName("ix_bill_settlements_payment_id");
    }
}
