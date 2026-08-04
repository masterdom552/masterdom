using Masterdom.Core.Identity.Entities.EmailVerification;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="EmailVerification"/>.
/// </summary>
public sealed class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
{
    public void Configure(EntityTypeBuilder<EmailVerification> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("EmailVerifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(EmailVerificationId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasEntityIdConversion(UserId.From)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.EmailAddress)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.RequestedAtUtc)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.VerifiedAtUtc);

        builder.Property(x => x.Status)
            .HasValueObjectConversion(EmailVerificationStatus.Create)
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

        builder.Ignore("DomainEvents");

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.EmailAddress);

        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.ExpiresAtUtc);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
