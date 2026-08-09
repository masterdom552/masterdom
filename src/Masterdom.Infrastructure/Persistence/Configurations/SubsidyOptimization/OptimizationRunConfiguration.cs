using System.Text.Json;
using Masterdom.Infrastructure.Persistence.Extensions;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Infrastructure.Persistence.Configurations.SubsidyOptimization;

public sealed class OptimizationRunConfiguration : IEntityTypeConfiguration<OptimizationRunAggregate>
{
    public void Configure(EntityTypeBuilder<OptimizationRunAggregate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("subsidy_optimization_runs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasEntityIdConversion(OptimizationRunId.From)
            .ValueGeneratedNever();

        builder.Property(x => x.Scenario)
            .HasConversion(
                value => JsonSerializer.Serialize(new ScenarioPersistenceModel(
                    value.ScenarioId.Value,
                    value.Name,
                    value.Description), JsonSerializerOptions.Web),
                json => DeserializeScenario(json))
            .HasColumnName("scenario")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.MeterGroup)
            .HasConversion(
                value => JsonSerializer.Serialize(
                    MeterGroupPersistenceModel.FromDomain(value),
                    JsonSerializerOptions.Web),
                json => DeserializeMeterGroup(json))
            .HasColumnName("meter_group")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.RatingReference)
            .HasConversion(
                value => JsonSerializer.Serialize(new RatingReferencePersistenceModel(value.RatingIds), JsonSerializerOptions.Web),
                json => DeserializeRatingReference(json))
            .HasColumnName("rating_reference")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OptimizationPeriod)
            .HasConversion(
                value => JsonSerializer.Serialize(new PeriodPersistenceModel(value.StartDate, value.EndDate), JsonSerializerOptions.Web),
                json => DeserializePeriod(json))
            .HasColumnName("optimization_period")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OptimizationStatus)
            .HasConversion(
                value => value.Value,
                value => OptimizationStatus.Create(value))
            .HasColumnName("optimization_status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.OptimizationVersion)
            .HasConversion(
                value => value.Value,
                value => OptimizationVersion.Create(value))
            .HasColumnName("optimization_version")
            .IsRequired();

        builder.Property(x => x.OptimizationResult)
            .HasConversion(
                value => value == null
                    ? null
                    : JsonSerializer.Serialize(new OptimizationResultPersistenceModel(value.EstimatedSavings, value.EstimatedCost, value.Summary), JsonSerializerOptions.Web),
                json => string.IsNullOrWhiteSpace(json)
                    ? null
                    : DeserializeOptimizationResult(json))
            .HasColumnName("optimization_result")
            .HasColumnType("jsonb");

        builder.Property(x => x.ConsumptionForecast)
            .HasConversion(
                value => value == null
                    ? null
                    : JsonSerializer.Serialize(new ConsumptionForecastPersistenceModel(value.BaselineConsumption, value.ProjectedConsumption, value.Assumptions), JsonSerializerOptions.Web),
                json => string.IsNullOrWhiteSpace(json)
                    ? null
                    : DeserializeConsumptionForecast(json))
            .HasColumnName("consumption_forecast")
            .HasColumnType("jsonb");

        builder.Property(x => x.ExecutionEvidence)
            .HasConversion(
                value => value == null
                    ? null
                    : JsonSerializer.Serialize(ExecutionEvidencePersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                json => string.IsNullOrWhiteSpace(json)
                    ? null
                    : DeserializeExecutionEvidence(json))
            .HasColumnName("execution_evidence")
            .HasColumnType("jsonb");

        builder.Property(x => x.StartedAtUtc)
            .HasColumnName("started_at_utc")
            .IsRequired();

        builder.Property(x => x.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        builder.OwnsMany(x => x.Recommendations, recommendationBuilder =>
        {
            recommendationBuilder.ToTable("optimization_recommendations");

            recommendationBuilder.WithOwner()
                .HasForeignKey("optimization_run_id");

            recommendationBuilder.Property<int>("id");
            recommendationBuilder.HasKey("id");

            recommendationBuilder.Property(x => x.RecommendationId)
                .HasConversion(
                    value => value.Value,
                    value => RecommendationId.From(value))
                .HasColumnName("recommendation_id")
                .IsRequired();

            recommendationBuilder.Property(x => x.Title)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();

            recommendationBuilder.Property(x => x.Details)
                .HasColumnName("details")
                .HasMaxLength(2000)
                .IsRequired();

            recommendationBuilder.Property(x => x.Priority)
                .HasConversion(
                    value => value.Value,
                    value => RecommendationPriority.Create(value))
                .HasColumnName("priority")
                .HasMaxLength(50)
                .IsRequired();

            recommendationBuilder.Property(x => x.GeneratedAtUtc)
                .HasColumnName("generated_at_utc")
                .IsRequired();

            recommendationBuilder.Property(x => x.IsArchived)
                .HasColumnName("is_archived")
                .IsRequired();

            recommendationBuilder.Property(x => x.ArchivedAtUtc)
                .HasColumnName("archived_at_utc");

            recommendationBuilder.Property(x => x.ArchivedReason)
                .HasColumnName("archived_reason")
                .HasMaxLength(1000);

            recommendationBuilder.HasIndex(x => x.RecommendationId)
                .HasDatabaseName("ix_optimization_recommendations_recommendation_id");
        });

        builder.OwnsMany(x => x.VersionHistory, versionBuilder =>
        {
            versionBuilder.ToTable("optimization_versions");

            versionBuilder.WithOwner()
                .HasForeignKey("optimization_run_id");

            versionBuilder.Property<int>("id");
            versionBuilder.HasKey("id");

            versionBuilder.Property(x => x.Version)
                .HasConversion(
                    value => value.Value,
                    value => OptimizationVersion.Create(value))
                .HasColumnName("version")
                .IsRequired();

            versionBuilder.Property(x => x.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();

            versionBuilder.HasIndex(x => x.Version)
                .HasDatabaseName("ix_optimization_versions_version");
        });

        builder.OwnsMany(x => x.Snapshots, snapshotBuilder =>
        {
            snapshotBuilder.ToTable("optimization_snapshots");

            snapshotBuilder.WithOwner()
                .HasForeignKey("optimization_run_id");

            snapshotBuilder.Property<int>("id");
            snapshotBuilder.HasKey("id");

            snapshotBuilder.Property(x => x.SnapshotId)
                .HasColumnName("snapshot_id")
                .IsRequired();

            snapshotBuilder.Property(x => x.Version)
                .HasConversion(
                    value => value.Value,
                    value => OptimizationVersion.Create(value))
                .HasColumnName("version")
                .IsRequired();

            snapshotBuilder.Property(x => x.CapturedAtUtc)
                .HasColumnName("captured_at_utc")
                .IsRequired();

            snapshotBuilder.Property(x => x.OptimizationResult)
                .HasConversion(
                    value => JsonSerializer.Serialize(new OptimizationResultPersistenceModel(value.EstimatedSavings, value.EstimatedCost, value.Summary), JsonSerializerOptions.Web),
                    json => DeserializeOptimizationResult(json))
                .HasColumnName("optimization_result")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.ConsumptionForecast)
                .HasConversion(
                    value => JsonSerializer.Serialize(new ConsumptionForecastPersistenceModel(value.BaselineConsumption, value.ProjectedConsumption, value.Assumptions), JsonSerializerOptions.Web),
                    json => DeserializeConsumptionForecast(json))
                .HasColumnName("consumption_forecast")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.RecommendationSet)
                .HasConversion(
                    value => JsonSerializer.Serialize(
                        value.Items.Select(RecommendationPersistenceModel.FromDomain).ToList(),
                        JsonSerializerOptions.Web),
                    json => DeserializeRecommendationSet(json))
                .HasColumnName("recommendation_set")
                .HasColumnType("jsonb")
                .IsRequired();

            snapshotBuilder.Property(x => x.ExecutionEvidence)
                .HasConversion(
                    value => value == null
                        ? null
                        : JsonSerializer.Serialize(ExecutionEvidencePersistenceModel.FromDomain(value), JsonSerializerOptions.Web),
                    json => string.IsNullOrWhiteSpace(json)
                        ? null
                        : DeserializeExecutionEvidence(json))
                .HasColumnName("execution_evidence")
                .HasColumnType("jsonb");

            snapshotBuilder.HasIndex(x => x.SnapshotId)
                .HasDatabaseName("ix_optimization_snapshots_snapshot_id");
        });

        builder.Navigation(x => x.Recommendations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.VersionHistory)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(x => x.Snapshots)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.RecommendationSet);
        builder.Ignore(x => x.DomainEvents);
    }

    private static SubsidyScenario DeserializeScenario(string json)
    {
        var model = JsonSerializer.Deserialize<ScenarioPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize optimization scenario.");

        return SubsidyScenario.Create(
            ScenarioId.Create(model.ScenarioId),
            model.Name,
            model.Description);
    }

    private static MeterGroup DeserializeMeterGroup(string json)
    {
        var model = JsonSerializer.Deserialize<MeterGroupPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize meter group.");

        return MeterGroup.Create(
            MeterGroupReference.Create(model.MeterGroupCode, model.MeterIds),
            model.DisplayName);
    }

    private static RatingReference DeserializeRatingReference(string json)
    {
        var model = JsonSerializer.Deserialize<RatingReferencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize rating reference.");

        return RatingReference.Create(model.RatingIds);
    }

    private static OptimizationPeriod DeserializePeriod(string json)
    {
        var model = JsonSerializer.Deserialize<PeriodPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize optimization period.");

        return OptimizationPeriod.Create(model.StartDate, model.EndDate);
    }

    private static OptimizationResult DeserializeOptimizationResult(string json)
    {
        var model = JsonSerializer.Deserialize<OptimizationResultPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize optimization result.");

        return OptimizationResult.Create(model.EstimatedSavings, model.EstimatedCost, model.Summary);
    }

    private static ConsumptionForecast DeserializeConsumptionForecast(string json)
    {
        var model = JsonSerializer.Deserialize<ConsumptionForecastPersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize consumption forecast.");

        return ConsumptionForecast.Create(model.BaselineConsumption, model.ProjectedConsumption, model.Assumptions);
    }

    private static RecommendationSet DeserializeRecommendationSet(string json)
    {
        var list = JsonSerializer.Deserialize<IReadOnlyList<RecommendationPersistenceModel>>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize recommendation set.");

        var recommendations = list
            .Select(x => x.ToDomain())
            .ToList();

        return RecommendationSet.Create(recommendations);
    }

    private static OptimizationExecutionEvidence DeserializeExecutionEvidence(string json)
    {
        var model = JsonSerializer.Deserialize<ExecutionEvidencePersistenceModel>(json, JsonSerializerOptions.Web)
            ?? throw new InvalidOperationException("Unable to deserialize optimization execution evidence.");

        return model.ToDomain();
    }

    private sealed record ScenarioPersistenceModel(string ScenarioId, string Name, string Description);

    private sealed record MeterGroupPersistenceModel(string MeterGroupCode, string DisplayName, IReadOnlyList<Guid> MeterIds)
    {
        public static MeterGroupPersistenceModel FromDomain(MeterGroup meterGroup)
        {
            return new MeterGroupPersistenceModel(
                meterGroup.Reference.MeterGroupCode,
                meterGroup.DisplayName,
                meterGroup.Reference.MeterIds);
        }
    }

    private sealed record RatingReferencePersistenceModel(IReadOnlyList<Guid> RatingIds);

    private sealed record PeriodPersistenceModel(DateOnly StartDate, DateOnly EndDate);

    private sealed record OptimizationResultPersistenceModel(decimal EstimatedSavings, decimal EstimatedCost, string Summary);

    private sealed record ConsumptionForecastPersistenceModel(decimal BaselineConsumption, decimal ProjectedConsumption, string Assumptions);

    private sealed record ExecutionEvidencePersistenceModel(
        string? TenantId,
        string? PropertyId,
        string ConfigurationContextVersion,
        DateTime EffectiveDateUtc,
        decimal OccupancyRate,
        decimal ConfidenceThreshold,
        string AlgorithmVersion,
        IReadOnlyList<OptimizationMeterInput> MeterInputs,
        IReadOnlyList<OptimizationRatingInput> RatingInputs,
        IReadOnlyList<OptimizationImportedDatasetInput> ImportedDatasets,
        OptimizationPolicySnapshot Policy,
        OptimizationModelSnapshot Model,
        OptimizationStrategySnapshot Strategy,
        IReadOnlyList<OptimizationScenarioEvidence> Scenarios,
        string SelectedScenarioCode,
        OptimizationOutcomeEvidence Outcome)
    {
        public static ExecutionEvidencePersistenceModel FromDomain(OptimizationExecutionEvidence evidence)
        {
            return new ExecutionEvidencePersistenceModel(
                evidence.TenantId,
                evidence.PropertyId,
                evidence.ConfigurationContextVersion,
                evidence.EffectiveDateUtc,
                evidence.OccupancyRate,
                evidence.ConfidenceThreshold,
                evidence.AlgorithmVersion,
                evidence.MeterInputs,
                evidence.RatingInputs,
                evidence.ImportedDatasets,
                evidence.Policy,
                evidence.Model,
                evidence.Strategy,
                evidence.Scenarios,
                evidence.SelectedScenarioCode,
                evidence.Outcome);
        }

        public OptimizationExecutionEvidence ToDomain()
        {
            return OptimizationExecutionEvidence.Create(
                TenantId,
                PropertyId,
                ConfigurationContextVersion,
                EffectiveDateUtc,
                OccupancyRate,
                ConfidenceThreshold,
                AlgorithmVersion,
                MeterInputs,
                RatingInputs,
                ImportedDatasets,
                Policy,
                Model,
                Strategy,
                Scenarios,
                SelectedScenarioCode,
                Outcome);
        }
    }

    private sealed record RecommendationPersistenceModel(
        Guid RecommendationId,
        string Title,
        string Details,
        string Priority,
        DateTime GeneratedAtUtc,
        bool IsArchived,
        DateTime? ArchivedAtUtc,
        string? ArchivedReason)
    {
        public static RecommendationPersistenceModel FromDomain(OptimizationRecommendation recommendation)
        {
            return new RecommendationPersistenceModel(
                recommendation.RecommendationId.Value,
                recommendation.Title,
                recommendation.Details,
                recommendation.Priority.Value,
                recommendation.GeneratedAtUtc,
                recommendation.IsArchived,
                recommendation.ArchivedAtUtc,
                recommendation.ArchivedReason);
        }

        public OptimizationRecommendation ToDomain()
        {
            var recommendation = OptimizationRecommendation.Generate(
                Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.RecommendationId.From(RecommendationId),
                Title,
                Details,
                RecommendationPriority.Create(Priority),
                GeneratedAtUtc);

            return IsArchived
                ? recommendation.Archive(ArchivedReason ?? "Archived", ArchivedAtUtc ?? GeneratedAtUtc)
                : recommendation;
        }
    }
}
