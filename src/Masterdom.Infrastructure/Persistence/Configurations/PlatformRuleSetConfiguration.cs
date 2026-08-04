using Masterdom.Infrastructure.Persistence.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformRuleSetConfiguration
    : IEntityTypeConfiguration<PlatformRuleSetEntity>
{
    public void Configure(EntityTypeBuilder<PlatformRuleSetEntity> builder)
    {
        builder.ToTable("platform_rule_sets");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Key)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Category)
            .IsRequired();

        builder.Property(x => x.ScopeKind)
            .IsRequired();

        builder.Property(x => x.ScopeIdentifier)
            .HasMaxLength(200);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc)
            .IsRequired();

        builder.Property(x => x.EffectiveToUtc);

        builder.Property(x => x.IsDeprecated)
            .IsRequired();

        builder.Property(x => x.ReplacedByKey)
            .HasMaxLength(250);

        builder.Property(x => x.Compatibility)
            .HasMaxLength(2000);

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Key,
            x.ScopeKind,
            x.ScopeIdentifier,
            x.Version,
            x.EffectiveFromUtc
        });
    }
}
