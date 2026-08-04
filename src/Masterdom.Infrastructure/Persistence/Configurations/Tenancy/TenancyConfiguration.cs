using Masterdom.Core.Identifiers;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Infrastructure.Persistence.Configurations.Tenancy;

/// <summary>
/// EF Core configuration for tenancy aggregate.
/// </summary>
public sealed class TenancyConfiguration : IEntityTypeConfiguration<TenancyAggregate>
{
    public void Configure(EntityTypeBuilder<TenancyAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tenancies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(TenancyId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Number)
            .HasValueObjectConversion(TenancyNumber.Create)
            .HasMaxLength(50)
            .HasColumnName("tenancy_number")
            .IsRequired();

        builder.HasIndex(x => x.Number)
            .IsUnique();

        builder.Property(x => x.Property)
            .HasConversion(
                value => value.PropertyId,
                value => PropertyReference.Create(value))
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasConversion(
                value => value.UnitId,
                value => UnitReference.Create(value))
            .HasColumnName("unit_id")
            .IsRequired();

        builder.Property(x => x.MoveInDate)
            .HasConversion(
                value => value.Value,
                value => MoveInDate.Create(value))
            .HasColumnName("move_in_date")
            .IsRequired();

        builder.Property(x => x.MoveOutDate)
            .HasConversion(
                value => value == null ? (DateOnly?)null : value.Value,
                value => value.HasValue ? MoveOutDate.Create(value.Value) : null)
            .HasColumnName("move_out_date");

        builder.Property(x => x.Status)
            .HasValueObjectConversion(TenancyStatus.Create)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OccupancyStatus)
            .HasValueObjectConversion(OccupancyStatus.Create)
            .HasColumnName("occupancy_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ClosedOn)
            .HasConversion(
                value => value == null ? (DateOnly?)null : value.Value,
                value => value.HasValue ? EffectiveDate.Create(value.Value) : null)
            .HasColumnName("closed_on");

        builder.Property(x => x.TerminationReason)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : TerminationReason.Create(value))
            .HasColumnName("termination_reason")
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasConversion(
                value => value == null ? null : value.Value,
                value => string.IsNullOrWhiteSpace(value) ? null : Notes.Create(value))
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.OwnsMany(x => x.Occupants, occupantBuilder =>
        {
            occupantBuilder.ToTable("tenancy_occupants");

            occupantBuilder.WithOwner()
                .HasForeignKey("tenancy_id");

            occupantBuilder.Property<int>("id");
            occupantBuilder.HasKey("id");

            occupantBuilder.Property(x => x.PersonId)
                .HasEntityIdConversion(PersonId.From)
                .HasColumnName("person_id")
                .IsRequired();

            occupantBuilder.Property(x => x.IsPrimary)
                .HasColumnName("is_primary")
                .IsRequired();

            occupantBuilder.HasIndex(x => new { x.PersonId, x.IsPrimary })
                .HasDatabaseName("ix_tenancy_occupants_person_primary");
        });

        builder.Navigation(x => x.Occupants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
