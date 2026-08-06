using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Inventory;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("inventory_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(InventoryItemId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.QuantityOnHand)
            .HasColumnName("quantity_on_hand")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => x.PropertyId)
            .HasDatabaseName("ix_inventory_items_property_id");

        builder.HasIndex(x => new { x.PropertyId, x.Sku })
            .HasDatabaseName("ux_inventory_items_property_id_sku")
            .IsUnique();

        builder.Ignore(x => x.DomainEvents);
    }
}
