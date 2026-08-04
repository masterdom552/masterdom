using Masterdom.Infrastructure.Persistence.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformWorkflowStepConfiguration
    : IEntityTypeConfiguration<PlatformWorkflowStepEntity>
{
    public void Configure(EntityTypeBuilder<PlatformWorkflowStepEntity> builder)
    {
        builder.ToTable("platform_workflow_steps");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.WorkflowVersionId)
            .IsRequired();

        builder.Property(x => x.Key)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Kind)
            .IsRequired();

        builder.Property(x => x.IsStart)
            .IsRequired();

        builder.Property(x => x.IsTerminal)
            .IsRequired();

        builder.Property(x => x.RetryMaxAttempts)
            .IsRequired();

        builder.Property(x => x.RetryDelayMilliseconds)
            .IsRequired();

        builder.Property(x => x.TimeoutMilliseconds)
            .IsRequired();

        builder.Property(x => x.CompensationOperation)
            .HasMaxLength(500);

        builder.HasIndex(x => x.WorkflowVersionId);

        builder.HasIndex(x => new
        {
            x.WorkflowVersionId,
            x.Key
        });
    }
}
