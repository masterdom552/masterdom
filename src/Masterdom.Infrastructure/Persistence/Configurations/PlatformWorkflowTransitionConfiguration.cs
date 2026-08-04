using Masterdom.Infrastructure.Persistence.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformWorkflowTransitionConfiguration
    : IEntityTypeConfiguration<PlatformWorkflowTransitionEntity>
{
    public void Configure(EntityTypeBuilder<PlatformWorkflowTransitionEntity> builder)
    {
        builder.ToTable("platform_workflow_transitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.WorkflowVersionId)
            .IsRequired();

        builder.Property(x => x.FromStepId)
            .IsRequired();

        builder.Property(x => x.ToStepId)
            .IsRequired();

        builder.Property(x => x.BranchKind)
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.ConditionKind)
            .IsRequired();

        builder.Property(x => x.RuleSetKey)
            .HasMaxLength(250);

        builder.Property(x => x.RuleScopeKind);

        builder.Property(x => x.RuleScopeIdentifier)
            .HasMaxLength(200);

        builder.HasIndex(x => x.WorkflowVersionId);

        builder.HasIndex(x => x.FromStepId);

        builder.HasIndex(x => new
        {
            x.WorkflowVersionId,
            x.FromStepId,
            x.Priority
        });
    }
}
