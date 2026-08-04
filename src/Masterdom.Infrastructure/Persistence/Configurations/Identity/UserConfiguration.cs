using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.Entities.IdentityProfile;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="User"/>.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(UserId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Code)
            .HasValueObjectConversion(UserCode.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.IdentityProfileId)
            .HasEntityIdConversion(IdentityProfileId.From)
            .IsRequired();

        builder.Property(x => x.Username)
            .HasValueObjectConversion(Username.Create)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(UserStatus.Create)
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

        builder.HasIndex(x => x.IdentityProfileId);

        builder.HasIndex(x => x.Status);

        builder.HasOne<IdentityProfile>()
            .WithMany()
            .HasForeignKey(x => x.IdentityProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
