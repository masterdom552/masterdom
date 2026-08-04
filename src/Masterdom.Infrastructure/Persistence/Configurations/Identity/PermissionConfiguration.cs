using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="Permission"/>.
/// </summary>
public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Permissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(PermissionId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasValueObjectConversion(PermissionCode.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasValueObjectConversion(PermissionName.Create)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(PermissionStatus.Create)
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

        builder.HasIndex(x => x.Status);
    }
}
