using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Modules.Properties.Infrastructure.Persistence.Configurations;

public sealed class StockLocationConfiguration : IEntityTypeConfiguration<StockLocation>
{
    public void Configure(EntityTypeBuilder<StockLocation> builder)
    {
        builder.ToTable("stock_locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("stock_location_id")
            .HasConversion(
                id => id.Value,
                value => new StockLocationId(value));

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(64)
            .IsRequired(false);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(x => x.PropertyId)
            .HasConversion(
                id => id.Value,
                value => new PropertyId(value))
            .HasColumnName("property_id")
            .IsRequired();

        builder.HasIndex(x => x.PropertyId)
            .HasDatabaseName("ix_stock_locations_property_id");

        builder.HasIndex(x => new { x.PropertyId, x.Name })
            .HasDatabaseName("ux_stock_locations_property_id_name")
            .IsUnique();
    }
}
