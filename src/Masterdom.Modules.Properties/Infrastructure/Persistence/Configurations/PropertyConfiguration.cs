using Masterdom.Modules.Properties.Domain.Entities.Property;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Modules.Properties.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new PropertyId(value));

        builder.Property(x => x.Code)
            .HasConversion(
                code => code.Value,
                value => new PropertyCode(value))
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasConversion(
                name => name.Value,
                value => new PropertyName(value))
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Type);

        builder.Property(x => x.Status);

        builder.Property(x => x.Description);

        builder.Property(x => x.Remarks);

        builder.Property(x => x.OwnerId);

        builder.Property(x => x.ParentPropertyId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new PropertyId(value.Value));

        builder.OwnsOne(
            x => x.Address,
            address =>
            {
                address.Property(x => x.Line1)
                    .HasMaxLength(200)
                    .HasColumnName("address_line1");

                address.Property(x => x.Line2)
                    .HasMaxLength(200)
                    .HasColumnName("address_line2");

                address.Property(x => x.City)
                    .HasMaxLength(120)
                    .HasColumnName("address_city");

                address.Property(x => x.StateOrProvince)
                    .HasMaxLength(120)
                    .HasColumnName("address_state_or_province");

                address.Property(x => x.PostalCode)
                    .HasMaxLength(40)
                    .HasColumnName("address_postal_code");

                address.Property(x => x.CountryCode)
                    .HasMaxLength(3)
                    .HasColumnName("address_country_code");
            });

        builder.OwnsOne(
            x => x.Settings,
            settings =>
            {
                settings.Property(x => x.TimeZoneId)
                    .HasMaxLength(80)
                    .HasColumnName("settings_time_zone_id")
                    .IsRequired();

                settings.Property(x => x.CurrencyCode)
                    .HasMaxLength(3)
                    .HasColumnName("settings_currency_code")
                    .IsRequired();

                settings.Property(x => x.AllowNegativeOccupancy)
                    .HasColumnName("settings_allow_negative_occupancy")
                    .IsRequired();
            });

        builder.OwnsMany(
            x => x.Metadata,
            metadata =>
            {
                metadata.ToTable("property_metadata");

                metadata.WithOwner()
                    .HasForeignKey("property_id");

                metadata.Property<int>("id");
                metadata.HasKey("id");

                metadata.Property(x => x.Key)
                    .HasColumnName("metadata_key")
                    .HasMaxLength(120)
                    .IsRequired();

                metadata.Property(x => x.Value)
                    .HasColumnName("metadata_value")
                    .HasMaxLength(500)
                    .IsRequired();

                metadata.HasIndex("property_id", nameof(PropertyMetadata.Key))
                    .IsUnique();
            });

        builder.OwnsMany(
            x => x.Relationships,
            relationship =>
            {
                relationship.ToTable("property_relationships");

                relationship.WithOwner()
                    .HasForeignKey("property_id");

                relationship.Property<int>("id");
                relationship.HasKey("id");

                relationship.Property(x => x.TargetPropertyId)
                    .HasConversion(
                        id => id.Value,
                        value => new PropertyId(value))
                    .HasColumnName("target_property_id")
                    .IsRequired();

                relationship.Property(x => x.Type)
                    .HasColumnName("relationship_type")
                    .IsRequired();
            });

        builder.Property(x => x.EffectiveFromUtc);

        builder.Property(x => x.EffectiveToUtc);

        builder.Property(x => x.DisplayOrder);

        builder.Property(x => x.IsHidden);

        builder.Ignore(x => x.DomainEvents);

        builder.Metadata
            .FindNavigation(nameof(Property.Units))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Metadata)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Relationships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Units)
            .WithOne()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
