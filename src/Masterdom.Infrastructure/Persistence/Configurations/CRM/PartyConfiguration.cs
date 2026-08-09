using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.CRM;

/// <summary>
/// EF Core configuration for <see cref="Party"/>.
/// </summary>
public sealed class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("crm_parties");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("party_id")
            .HasEntityIdConversion(PartyId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.LegalName)
            .HasColumnName("legal_name")
            .HasMaxLength(200);

        builder.Property(x => x.PartyType)
            .HasColumnName("party_type")
            .HasValueObjectConversion(PartyType.Create)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasValueObjectConversion(PartyStatus.Create)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired();

        builder.OwnsOne(x => x.AuditInfo, auditBuilder =>
        {
            auditBuilder.Property(x => x.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(100);

            auditBuilder.Property(x => x.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(100);
        });

        builder.Navigation(x => x.AuditInfo)
            .IsRequired();

        builder.OwnsMany(x => x.ContactMethods, contactBuilder =>
        {
            contactBuilder.ToTable("crm_party_contact_methods");

            contactBuilder.WithOwner()
                .HasForeignKey("party_id");

            contactBuilder.Property<int>("id");
            contactBuilder.HasKey("id");

            contactBuilder.Property(x => x.Type)
                .HasColumnName("contact_type")
                .HasValueObjectConversion(ContactMethodType.Create)
                .HasMaxLength(64)
                .IsRequired();

            contactBuilder.Property(x => x.Value)
                .HasColumnName("contact_value")
                .HasMaxLength(256)
                .IsRequired();

            contactBuilder.Property(x => x.IsPreferred)
                .HasColumnName("is_preferred")
                .IsRequired();

            contactBuilder.HasIndex("party_id", nameof(ContactMethod.Type), nameof(ContactMethod.Value))
                .IsUnique();
        });

        builder.Navigation(x => x.ContactMethods)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Addresses, addressBuilder =>
        {
            addressBuilder.ToTable("crm_party_addresses");

            addressBuilder.WithOwner()
                .HasForeignKey("party_id");

            addressBuilder.Property<int>("id");
            addressBuilder.HasKey("id");

            addressBuilder.Property(x => x.Type)
                .HasColumnName("address_type")
                .HasValueObjectConversion(AddressType.Create)
                .HasMaxLength(64)
                .IsRequired();

            addressBuilder.Property(x => x.Line1)
                .HasColumnName("line1")
                .HasMaxLength(200)
                .IsRequired();

            addressBuilder.Property(x => x.Line2)
                .HasColumnName("line2")
                .HasMaxLength(200);

            addressBuilder.Property(x => x.City)
                .HasColumnName("city")
                .HasMaxLength(120)
                .IsRequired();

            addressBuilder.Property(x => x.StateOrProvince)
                .HasColumnName("state_or_province")
                .HasMaxLength(120)
                .IsRequired();

            addressBuilder.Property(x => x.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(32)
                .IsRequired();

            addressBuilder.Property(x => x.Country)
                .HasColumnName("country")
                .HasMaxLength(120)
                .IsRequired();

            addressBuilder.Property(x => x.IsPreferred)
                .HasColumnName("is_preferred")
                .IsRequired();
        });

        builder.Navigation(x => x.Addresses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.Relationships, relationshipBuilder =>
        {
            relationshipBuilder.ToTable("crm_party_relationships");

            relationshipBuilder.WithOwner()
                .HasForeignKey("party_id");

            relationshipBuilder.Property<int>("id");
            relationshipBuilder.HasKey("id");

            relationshipBuilder.Property(x => x.RelatedPartyId)
                .HasColumnName("related_party_id")
                .HasEntityIdConversion(PartyId.From)
                .IsRequired();

            relationshipBuilder.Property(x => x.Type)
                .HasColumnName("relationship_type")
                .HasValueObjectConversion(RelationshipType.Create)
                .HasMaxLength(64)
                .IsRequired();

            relationshipBuilder.Property(x => x.AllowsSelfReference)
                .HasColumnName("allows_self_reference")
                .IsRequired();

            relationshipBuilder.HasIndex("party_id", nameof(Relationship.RelatedPartyId), nameof(Relationship.Type))
                .IsUnique();
        });

        builder.Navigation(x => x.Relationships)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.RoleAssignments, roleBuilder =>
        {
            roleBuilder.ToTable("crm_party_role_assignments");

            roleBuilder.WithOwner()
                .HasForeignKey("party_id");

            roleBuilder.Property(x => x.Id)
                .HasColumnName("party_role_assignment_id")
                .HasEntityIdConversion(PartyRoleAssignmentId.From)
                .ValueGeneratedNever();

            roleBuilder.HasKey(x => x.Id);

            roleBuilder.Property(x => x.RoleType)
                .HasColumnName("role_type")
                .HasValueObjectConversion(PartyRoleType.Create)
                .HasMaxLength(64)
                .IsRequired();

            roleBuilder.Property(x => x.AssignedAtUtc)
                .HasColumnName("assigned_at_utc")
                .IsRequired();

            roleBuilder.Property(x => x.EffectiveFromUtc)
                .HasColumnName("effective_from_utc")
                .IsRequired();

            roleBuilder.Property(x => x.EffectiveToUtc)
                .HasColumnName("effective_to_utc");

            roleBuilder.Property(x => x.AssignmentReason)
                .HasColumnName("assignment_reason")
                .HasMaxLength(1000);

            roleBuilder.Property(x => x.Status)
                .HasColumnName("status")
                .HasValueObjectConversion(PartyRoleAssignmentStatus.Create)
                .HasMaxLength(32)
                .IsRequired();

            roleBuilder.Property(x => x.DeactivatedAtUtc)
                .HasColumnName("deactivated_at_utc");

            roleBuilder.Property(x => x.DeactivationReason)
                .HasColumnName("deactivation_reason")
                .HasMaxLength(1000);

            roleBuilder.Property(x => x.RemovedAtUtc)
                .HasColumnName("removed_at_utc");

            roleBuilder.Property(x => x.RemovalReason)
                .HasColumnName("removal_reason")
                .HasMaxLength(1000);

            roleBuilder.Property(x => x.ReactivatedAtUtc)
                .HasColumnName("reactivated_at_utc");

            roleBuilder.Property(x => x.ReactivationReason)
                .HasColumnName("reactivation_reason")
                .HasMaxLength(1000);

            roleBuilder.HasIndex("party_id", nameof(PartyRoleAssignment.RoleType), nameof(PartyRoleAssignment.Status));
            roleBuilder.HasIndex(nameof(PartyRoleAssignment.RoleType), nameof(PartyRoleAssignment.Status));
        });

        builder.Navigation(x => x.RoleAssignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.DomainEvents);
    }
}
