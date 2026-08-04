using Masterdom.Infrastructure.Persistence.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformWorkflowVersionConfiguration
    : IEntityTypeConfiguration<PlatformWorkflowVersionEntity>
{
    public void Configure(EntityTypeBuilder<PlatformWorkflowVersionEntity> builder)
    {
        builder.ToTable("platform_workflow_versions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.WorkflowId)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc)
            .IsRequired();

        builder.Property(x => x.EffectiveToUtc);

        builder.Property(x => x.IsDeprecated)
            .IsRequired();

        builder.Property(x => x.ReplacedByVersionId);

        builder.Property(x => x.Compatibility)
            .HasMaxLength(2000);

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.WorkflowId);

        builder.HasIndex(x => new
        {
            x.WorkflowId,
            x.Version,
            x.EffectiveFromUtc
        });
    }
}
