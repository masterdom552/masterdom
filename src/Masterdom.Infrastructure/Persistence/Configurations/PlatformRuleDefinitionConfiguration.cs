using Masterdom.Infrastructure.Persistence.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformRuleDefinitionConfiguration
    : IEntityTypeConfiguration<PlatformRuleDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<PlatformRuleDefinitionEntity> builder)
    {
        builder.ToTable("platform_rule_definitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.RuleSetId)
            .IsRequired();

        builder.Property(x => x.ParentRuleId);

        builder.Property(x => x.Key)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Kind)
            .IsRequired();

        builder.Property(x => x.Category)
            .IsRequired();

        builder.Property(x => x.Severity)
            .IsRequired();

        builder.Property(x => x.Priority)
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

        builder.Property(x => x.InputKey)
            .HasMaxLength(300);

        builder.Property(x => x.CompareInputKey)
            .HasMaxLength(300);

        builder.Property(x => x.ExpectedText)
            .HasMaxLength(2000);

        builder.Property(x => x.ExpressionLeftKey)
            .HasMaxLength(300);

        builder.Property(x => x.ExpressionRightKey)
            .HasMaxLength(300);

        builder.HasIndex(x => x.RuleSetId);

        builder.HasIndex(x => x.ParentRuleId);

        builder.HasIndex(x => new
        {
            x.RuleSetId,
            x.Key,
            x.ScopeKind,
            x.ScopeIdentifier,
            x.Version,
            x.EffectiveFromUtc
        });
    }
}
