using Masterdom.Core.Identity.Entities.Relationship;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="Relationship"/>.
/// </summary>
public sealed class RelationshipConfiguration
    : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Relationships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(RelationshipId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasValueObjectConversion(RelationshipCode.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.FromIdentityProfileId)
            .HasEntityIdConversion(IdentityProfileId.From)
            .IsRequired();

        builder.Property(x => x.ToIdentityProfileId)
            .HasEntityIdConversion(IdentityProfileId.From)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasValueObjectConversion(RelationshipType.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(RelationshipStatus.Create)
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

        builder.HasIndex(x => x.FromIdentityProfileId);

        builder.HasIndex(x => x.ToIdentityProfileId);

        builder.HasIndex(x => x.Type);

        builder.HasIndex(x => x.Status);

        builder.HasOne<IdentityProfile>()
            .WithMany()
            .HasForeignKey(x => x.FromIdentityProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<IdentityProfile>()
            .WithMany()
            .HasForeignKey(x => x.ToIdentityProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
