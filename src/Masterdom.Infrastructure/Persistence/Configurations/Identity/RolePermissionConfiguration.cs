using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.RolePermission;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="RolePermission"/>.
/// </summary>
public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("RolePermissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(RolePermissionId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.RoleId)
            .HasEntityIdConversion(RoleId.From)
            .IsRequired();

        builder.Property(x => x.PermissionId)
            .HasEntityIdConversion(PermissionId.From)
            .IsRequired();

        builder.Property(x => x.AssignedAtUtc)
            .IsRequired();

        builder.Property(x => x.AssignedBy)
            .HasEntityIdConversion(UserId.From);

        builder.Property(x => x.AssignmentReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(RolePermissionStatus.Create)
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

        builder.Property(x => x.RevokedAtUtc);

        builder.Property(x => x.RevokedBy)
            .HasEntityIdConversion(UserId.From);

        builder.Property(x => x.RevocationReason)
            .HasMaxLength(1000);

        builder.Ignore("DomainEvents");

        builder.HasIndex(x => new { x.RoleId, x.PermissionId })
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
