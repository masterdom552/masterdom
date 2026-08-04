using System.Text.Json;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Infrastructure.Persistence.Configurations.PolicyFramework;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<PolicyAggregate>
{
    public void Configure(EntityTypeBuilder<PolicyAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("policies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(PolicyId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.PolicyType)
            .HasConversion(
                value => value.Value,
                value => PolicyType.Create(value))
            .HasColumnName("policy_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PolicyCategory)
            .HasConversion(
                value => value.Value,
                value => PolicyCategory.Create(value))
            .HasColumnName("policy_category")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PolicyReference)
            .HasConversion(
                value => JsonSerializer.Serialize(new PolicyReferencePersistenceModel(value.PolicyCode, value.DisplayName), JsonSerializerOptions.Web),
                json => DeserializePolicyReference(json))
            .HasColumnName("policy_reference")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Scope)
            .HasConversion(
                value => JsonSerializer.Serialize(new PolicyScopePersistenceModel(value.Kind.Value, value.ScopeKey), JsonSerializerOptions.Web),
                json => DeserializePolicyScope(json))
            .HasColumnName("policy_scope")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                value => value.Value,
                value => PolicyStatus.Create(value))
            .HasColumnName("policy_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(x => x.ActivatedAtUtc)
            .HasColumnName("activated_at_utc");

        builder.Property(x => x.ExpiredAtUtc)
            .HasColumnName("expired_at_utc");

        builder.Property(x => x.ArchivedAtUtc)
            .HasColumnName("archived_at_utc");

        builder.Property(x => x.ArchivedReason)
            .HasColumnName("archived_reason")
            .HasMaxLength(1000);

        builder.OwnsMany(x => x.Versions, versionBuilder =>
        {
            versionBuilder.ToTable("policy_versions");

            versionBuilder.WithOwner()
                .HasForeignKey("policy_id");

            versionBuilder.Property<int>("id");
            versionBuilder.HasKey("id");

            versionBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            versionBuilder.Property(x => x.EffectiveDateRange)
                .HasConversion(
                    value => JsonSerializer.Serialize(new EffectiveDateRangePersistenceModel(value.StartDate, value.EndDate), JsonSerializerOptions.Web),
                    json => DeserializeEffectiveDateRange(json))
                .HasColumnName("effective_date_range")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.Property(x => x.Condition)
                .HasConversion(
                    value => JsonSerializer.Serialize(new PolicyConditionPersistenceModel(value.SelectorKey, value.SelectorDefinition), JsonSerializerOptions.Web),
                    json => DeserializePolicyCondition(json))
                .HasColumnName("policy_condition")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.Property(x => x.Metadata)
                .HasConversion(
                    value => JsonSerializer.Serialize(value.Attributes, JsonSerializerOptions.Web),
                    json => DeserializePolicyMetadata(json))
                .HasColumnName("policy_metadata")
                .HasColumnType("jsonb")
                .IsRequired();

            versionBuilder.Property(x => x.Status)
                .HasConversion(
                    value => value.Value,
                    value => PolicyStatus.Create(value))
                .HasColumnName("policy_status")
                .HasMaxLength(50)
                .IsRequired();

            versionBuilder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            versionBuilder.Property(x => x.ActivatedAtUtc)
                .HasColumnName("activated_at_utc");

            versionBuilder.Property(x => x.ExpiredAtUtc)
                .HasColumnName("expired_at_utc");

            versionBuilder.HasIndex(x => x.VersionNumber)
                .HasDatabaseName("ix_policy_versions_version_number");
        });

        builder.OwnsMany(x => x.Assignments, assignmentBuilder =>
        {
            assignmentBuilder.ToTable("policy_assignments");

            assignmentBuilder.WithOwner()
                .HasForeignKey("policy_id");

            assignmentBuilder.Property<int>("id");
            assignmentBuilder.HasKey("id");

            assignmentBuilder.Property(x => x.AssignmentId)
                .HasColumnName("assignment_id")
                .IsRequired();

            assignmentBuilder.Property(x => x.Scope)
                .HasConversion(
                    value => JsonSerializer.Serialize(new PolicyScopePersistenceModel(value.Kind.Value, value.ScopeKey), JsonSerializerOptions.Web),
                    json => DeserializePolicyScope(json))
                .HasColumnName("policy_scope")
                .HasColumnType("jsonb")
                .IsRequired();

            assignmentBuilder.Property(x => x.AssignedEntityType)
                .HasColumnName("assigned_entity_type")
                .HasMaxLength(100)
                .IsRequired();

            assignmentBuilder.Property(x => x.AssignedEntityId)
                .HasColumnName("assigned_entity_id")
                .HasMaxLength(200)
                .IsRequired();

            assignmentBuilder.Property(x => x.EffectiveDateRange)
                .HasConversion(
                    value => JsonSerializer.Serialize(new EffectiveDateRangePersistenceModel(value.StartDate, value.EndDate), JsonSerializerOptions.Web),
                    json => DeserializeEffectiveDateRange(json))
                .HasColumnName("effective_date_range")
                .HasColumnType("jsonb")
                .IsRequired();

            assignmentBuilder.Property(x => x.AssignedAtUtc)
                .HasColumnName("assigned_at_utc")
                .IsRequired();

            assignmentBuilder.HasIndex(x => x.AssignmentId)
                .HasDatabaseName("ix_policy_assignments_assignment_id");
        });

        builder.OwnsMany(x => x.Snapshots, snapshotBuilder =>
        {
            snapshotBuilder.ToTable("policy_snapshots");

            snapshotBuilder.WithOwner()
                .HasForeignKey("policy_id");

            snapshotBuilder.Property<int>("id");
            snapshotBuilder.HasKey("id");

            snapshotBuilder.Property(x => x.SnapshotId)
                .HasColumnName("snapshot_id")
                .IsRequired();

            snapshotBuilder.Property(x => x.VersionNumber)
                .HasColumnName("version_number")
                .IsRequired();

            snapshotBuilder.Property(x => x.PolicyStatus)
                .HasConversion(
                    value => value.Value,
                    value => PolicyStatus.Create(value))
                .HasColumnName("policy_status")
                .HasMaxLength(50)
                .IsRequired();

            snapshotBuilder.Property(x => x.EffectiveDateRange)
                .HasConversion(
                    value => JsonSerializer.Serialize(new EffectiveDateRangePersistenceModel(value.StartDate, value.EndDate), JsonSerializerOptions.Web),
                    json => DeserializeEffectiveDateRange(json))
                .HasColumnName("effective_date_range")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.Condition)
                .HasConversion(
                    value => JsonSerializer.Serialize(new PolicyConditionPersistenceModel(value.SelectorKey, value.SelectorDefinition), JsonSerializerOptions.Web),
                    json => DeserializePolicyCondition(json))
                .HasColumnName("policy_condition")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.Metadata)
                .HasConversion(
                    value => JsonSerializer.Serialize(value.Attributes, JsonSerializerOptions.Web),
                    json => DeserializePolicyMetadata(json))
                .HasColumnName("policy_metadata")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.CapturedAtUtc)
                .HasColumnName("captured_at_utc")
                .IsRequired();

            snapshotBuilder.HasIndex(x => x.SnapshotId)
                .HasDatabaseName("ix_policy_snapshots_snapshot_id");
        });

        builder.Navigation(x => x.Versions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Assignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Snapshots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.CurrentVersion);
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.PolicyType)
            .HasDatabaseName("ix_policies_policy_type");

        builder.HasIndex(x => x.PolicyCategory)
            .HasDatabaseName("ix_policies_policy_category");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_policies_policy_status");
    }

    private static PolicyReference DeserializePolicyReference(string json)
    {
        var model = JsonSerializer.Deserialize<PolicyReferencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize policy reference.");

        return PolicyReference.Create(model.PolicyCode, model.DisplayName);
    }

    private static PolicyScope DeserializePolicyScope(string json)
    {
        var model = JsonSerializer.Deserialize<PolicyScopePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize policy scope.");

        return PolicyScope.Create(PolicyScopeKind.Create(model.ScopeKind), model.ScopeKey);
    }

    private static EffectiveDateRange DeserializeEffectiveDateRange(string json)
    {
        var model = JsonSerializer.Deserialize<EffectiveDateRangePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize effective date range.");

        return EffectiveDateRange.Create(model.StartDate, model.EndDate);
    }

    private static PolicyCondition DeserializePolicyCondition(string json)
    {
        var model = JsonSerializer.Deserialize<PolicyConditionPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize policy condition.");

        return PolicyCondition.Create(model.SelectorKey, model.SelectorDefinition);
    }

    private static PolicyMetadata DeserializePolicyMetadata(string json)
    {
        var model = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize policy metadata.");

        return PolicyMetadata.Create(model);
    }

    private sealed record PolicyReferencePersistenceModel(string PolicyCode, string DisplayName);

    private sealed record PolicyScopePersistenceModel(string ScopeKind, string ScopeKey);

    private sealed record EffectiveDateRangePersistenceModel(DateOnly StartDate, DateOnly? EndDate);

    private sealed record PolicyConditionPersistenceModel(string SelectorKey, string SelectorDefinition);
}
