using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Maintenance;

public sealed class MaintenanceTicketConfiguration : IEntityTypeConfiguration<MaintenanceTicket>
{
    public void Configure(EntityTypeBuilder<MaintenanceTicket> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("maintenance_tickets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(MaintenanceTicketId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.PropertyId)
            .HasColumnName("property_id")
            .IsRequired();

        builder.Property(x => x.UnitId)
            .HasColumnName("unit_id")
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(MaintenanceTicketStatus.Create)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.AssignedToPersonId)
            .HasColumnName("assigned_to_person_id");

        builder.Property(x => x.AssignedAtUtc)
            .HasColumnName("assigned_at_utc");

        builder.HasIndex(x => x.PropertyId)
            .HasDatabaseName("ix_maintenance_tickets_property_id");

        builder.HasIndex(x => x.UnitId)
            .HasDatabaseName("ix_maintenance_tickets_unit_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_maintenance_tickets_status");

        builder.HasIndex(x => x.AssignedToPersonId)
            .HasDatabaseName("ix_maintenance_tickets_assigned_to_person_id");

        builder.Ignore(x => x.DomainEvents);
    }
}
