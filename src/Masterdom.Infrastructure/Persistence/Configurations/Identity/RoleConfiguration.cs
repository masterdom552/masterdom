using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="Role"/>.
/// </summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(RoleId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasValueObjectConversion(RoleCode.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasValueObjectConversion(RoleName.Create)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(RoleStatus.Create)
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

        builder.Ignore(x => x.PermissionIds);

        builder.Ignore("DomainEvents");

        builder.HasIndex(x => x.Status);
    }
}
