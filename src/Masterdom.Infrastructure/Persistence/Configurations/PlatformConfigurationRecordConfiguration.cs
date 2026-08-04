using Masterdom.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masterdom.Infrastructure.Persistence.Configurations;

internal sealed class PlatformConfigurationRecordConfiguration
    : IEntityTypeConfiguration<PlatformConfigurationRecordEntity>
{
    public void Configure(EntityTypeBuilder<PlatformConfigurationRecordEntity> builder)
    {
        builder.ToTable("platform_configuration_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Key)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.ScopeKind)
            .IsRequired();

        builder.Property(x => x.ScopeIdentifier)
            .HasMaxLength(200);

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.Value)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.EffectiveFromUtc)
            .IsRequired();

        builder.Property(x => x.EffectiveToUtc);

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.ChangedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.Key,
            x.ScopeKind,
            x.ScopeIdentifier,
            x.EffectiveFromUtc,
            x.Version
        });
    }
}
