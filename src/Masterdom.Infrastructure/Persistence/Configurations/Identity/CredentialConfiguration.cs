using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations.Identity;

/// <summary>
/// EF Core configuration for <see cref="Credential"/>.
/// </summary>
public sealed class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("Credentials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(CredentialId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasEntityIdConversion(UserId.From)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasValueObjectConversion(CredentialStatus.Create)
            .HasMaxLength(50)
            .IsRequired();

        builder.Ignore("DomainEvents");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
