using Masterdom.Core.Identity.Entities.LoginAttempt;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="LoginAttempt"/>.
/// </summary>
public sealed class LoginAttemptConfiguration : IEntityTypeConfiguration<LoginAttempt>
{
    public void Configure(EntityTypeBuilder<LoginAttempt> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("LoginAttempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(LoginAttemptId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasEntityIdConversion(UserId.From);

        builder.Property(x => x.Username)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IpAddress)
            .HasMaxLength(100);

        builder.Property(x => x.ClientName)
            .HasMaxLength(1000);

        builder.Property(x => x.AttemptedAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(LoginAttemptStatus.Create)
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

        builder.HasIndex(x => x.Username);

        builder.HasIndex(x => x.AttemptedAtUtc);

        builder.HasIndex(x => x.Status);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
