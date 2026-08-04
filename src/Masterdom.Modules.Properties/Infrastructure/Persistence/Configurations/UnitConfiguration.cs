using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Modules.Properties.Infrastructure.Persistence.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("property_units");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new UnitId(value));

        builder.Property(x => x.Code)
            .HasConversion(
                code => code.Value,
                value => new UnitCode(value))
            .IsRequired();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => new UnitName(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type);

        builder.Property(x => x.Status);

        builder.Property(x => x.Description);

        builder.Property(x => x.Remarks);

        builder.Property(x => x.DisplayOrder);

        builder.Property(x => x.IsHidden);

        builder.Property(x => x.Capacity)
            .HasConversion(
                capacity => capacity.Value,
                value => new Capacity(value))
            .HasColumnName("capacity")
            .IsRequired();

        builder.Property(x => x.ParentUnitId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new UnitId(value.Value))
            .HasColumnName("parent_unit_id");

        builder.Property(x => x.PropertyId)
            .HasConversion(
                id => id.Value,
                value => new PropertyId(value))
            .IsRequired();

        builder.HasIndex(x => new { x.PropertyId, x.Code })
            .IsUnique();
    }

}
