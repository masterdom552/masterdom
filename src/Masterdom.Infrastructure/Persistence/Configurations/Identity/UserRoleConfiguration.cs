using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.UserRole;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="UserRole"/>.
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("UserRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(UserRoleId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasEntityIdConversion(UserId.From)
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasEntityIdConversion(RoleId.From)
            .IsRequired();

        builder.Property(x => x.AssignedAtUtc)
            .IsRequired();

        builder.Property(x => x.AssignedBy)
            .HasEntityIdConversion(UserId.From);

        builder.Property(x => x.IsPrimaryRole)
            .IsRequired();

        builder.Property(x => x.AssignmentReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(UserRoleStatus.Create)
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

        builder.HasIndex(x => new { x.UserId, x.RoleId })
            .IsUnique();

        builder.HasIndex(x => x.Status);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
