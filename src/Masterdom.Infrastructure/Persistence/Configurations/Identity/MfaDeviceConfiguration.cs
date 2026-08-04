using Masterdom.Core.Identity.Entities.MfaDevice;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="MfaDevice"/>.
/// </summary>
public sealed class MfaDeviceConfiguration : IEntityTypeConfiguration<MfaDevice>
{
    public void Configure(EntityTypeBuilder<MfaDevice> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("MfaDevices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(MfaDeviceId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasEntityIdConversion(UserId.From)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasValueObjectConversion(MfaDeviceType.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.SecretHash)
            .HasMaxLength(500);

        builder.Property(x => x.RegisteredAtUtc)
            .IsRequired();

        builder.Property(x => x.VerifiedAtUtc);

        builder.Property(x => x.LastUsedAtUtc);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(MfaDeviceStatus.Create)
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

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.Status);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
