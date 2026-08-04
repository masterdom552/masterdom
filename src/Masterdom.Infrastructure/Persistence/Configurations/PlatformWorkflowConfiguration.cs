using Masterdom.Infrastructure.Persistence.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformWorkflowConfiguration
    : IEntityTypeConfiguration<PlatformWorkflowEntity>
{
    public void Configure(EntityTypeBuilder<PlatformWorkflowEntity> builder)
    {
        builder.ToTable("platform_workflows");

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

        builder.Property(x => x.ScopeKind)
            .IsRequired();

        builder.Property(x => x.ScopeIdentifier)
            .HasMaxLength(200);

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Key,
            x.ScopeKind,
            x.ScopeIdentifier
        });
    }
}
