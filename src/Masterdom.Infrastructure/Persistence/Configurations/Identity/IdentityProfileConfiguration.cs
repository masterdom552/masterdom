using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Core.Identity.Entities.Organization;
using Masterdom.Core.Identifiers;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="IdentityProfile"/>.
/// </summary>
public sealed class IdentityProfileConfiguration
    : IEntityTypeConfiguration<IdentityProfile>
{
    public void Configure(EntityTypeBuilder<IdentityProfile> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("IdentityProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(IdentityProfileId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasValueObjectConversion(IdentityProfileCode.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Type)
            .HasValueObjectConversion(IdentityProfileType.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PersonId)
            .HasEntityIdConversion(PersonId.From);

        builder.Property(x => x.OrganizationId)
            .HasEntityIdConversion(OrganizationId.From);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(IdentityProfileStatus.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Remarks)
            .HasMaxLength(2000);

        builder.Property(x => x.Other)
            .HasMaxLength(2000);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsHidden)
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc);

        builder.Property(x => x.EffectiveToUtc);

        builder.Ignore("DomainEvents");

        builder.HasIndex(x => x.PersonId);

        builder.HasIndex(x => x.OrganizationId);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.Type);
    }
}
