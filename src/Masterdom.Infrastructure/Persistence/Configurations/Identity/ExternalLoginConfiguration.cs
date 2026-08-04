using Masterdom.Core.Identity.Entities.ExternalLogin;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="ExternalLogin"/>.
/// </summary>
public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("ExternalLogins");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(ExternalLoginId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasEntityIdConversion(UserId.From)
            .IsRequired();

        builder.Property(x => x.Provider)
            .HasValueObjectConversion(ExternalLoginProvider.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ProviderUserId)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.LinkedAtUtc)
            .IsRequired();

        builder.Property(x => x.LastUsedAtUtc);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(ExternalLoginStatus.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Remarks)
            .HasMaxLength(2000);

        builder.Property(x => x.Other)
            .HasMaxLength(2000);

        builder.Property(x => x.DisplayOrder).IsRequired();

        builder.Property(x => x.IsHidden).IsRequired();

        builder.Ignore("DomainEvents");

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
