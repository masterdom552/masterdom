using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core configuration for DelegatedAuthority aggregate.
/// </summary>
internal sealed class DelegatedAuthorityConfiguration : IEntityTypeConfiguration<DelegatedAuthority>
{
    public void Configure(EntityTypeBuilder<DelegatedAuthority> builder)
    {
        builder.ToTable("DelegatedAuthority", schema: "identity");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, x => DelegatedAuthorityId.From(x))
            .ValueGeneratedNever();

        builder.Property(x => x.DelegatorUserId)
            .HasConversion(x => x.Value, x => UserId.From(x))
            .IsRequired();

        builder.Property(x => x.DelegatedToUserId)
            .HasConversion(x => x.Value, x => UserId.From(x))
            .IsRequired();

        builder.Property(x => x.DelegatedRoleId)
            .HasConversion(x => x.Value, x => RoleId.From(x))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.EffectiveFromUtc)
            .IsRequired();

        builder.Property(x => x.EffectiveToUtc);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc);

        builder.Property(x => x.RevokedBy)
            .HasConversion(
                x => x == null ? null : (Guid?)x.Value,
                x => x == null ? null : UserId.From(x.Value));

        builder.Property(x => x.RevocationReason)
            .HasMaxLength(1024);

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.Property(x => x.Remarks)
            .HasMaxLength(2048);

        // Configure Scope as JSON (value object)
        builder.Property(x => x.Scope)
            .HasColumnType("jsonb")
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<DelegationScope>(v, (System.Text.Json.JsonSerializerOptions)null)!);

        builder.HasIndex(x => x.DelegatorUserId);
        builder.HasIndex(x => x.DelegatedToUserId);
        builder.HasIndex(x => x.DelegatedRoleId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.DelegatedToUserId, x.Status });
        builder.HasIndex(x => new { x.DelegatedToUserId, x.EffectiveFromUtc, x.EffectiveToUtc });
    }
}
