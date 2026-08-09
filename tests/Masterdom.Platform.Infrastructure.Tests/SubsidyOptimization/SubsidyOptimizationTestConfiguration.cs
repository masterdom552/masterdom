using System.Text.Json;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.Infrastructure.Tests.SubsidyOptimization;

internal static class SubsidyOptimizationTestConfiguration
{
    public static IReadOnlyList<ConfigurationRecord> CreateRecords(
        int version = 1,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        decimal firstSlabSubsidy = 100m)
    {
        var effectiveFrom = effectiveFromUtc ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return
        [
            CreateRecord(
                "subsidyoptimization.catalog.policy",
                version,
                effectiveFrom,
                effectiveToUtc,
                new SubsidyPolicyConfiguration(
                    "delhi-residential",
                    [
                        new SubsidySlabConfiguration(100m, firstSlabSubsidy, true),
                        new SubsidySlabConfiguration(200m, 50m, true),
                        new SubsidySlabConfiguration(decimal.MaxValue, 0m, false)
                    ],
                    SanctionedLoadLimit: 115m,
                    SanctionedLoadPenaltyPerUnit: 2m,
                    EligibleMeterTypes: ["residential"])),
            CreateRecord(
                "subsidyoptimization.catalog.optimization-model",
                version,
                effectiveFrom,
                effectiveToUtc,
                new OptimizationModelConfiguration(
                    "balanced-v1",
                    SubsidyWeight: 1m,
                    CostWeight: 1m,
                    LoadImpactWeight: 1m,
                    RiskWeight: 0.1m,
                    BoundaryTolerance: 0.01m,
                    MaximumScenarioCount: 20)),
            CreateRecord(
                "subsidyoptimization.catalog.optimization-strategy",
                version,
                effectiveFrom,
                effectiveToUtc,
                new OptimizationStrategyConfiguration(
                    "bounded-cliff-search-v1",
                    ConsumptionFactors: [1m, 0.98m, 0.95m, 0.90m],
                    IncludeSubsidyBoundaries: true,
                    PermitCrossMeterMovement: false,
                    MaximumCrossMeterMovementFraction: 0m))
        ];
    }

    private static ConfigurationRecord CreateRecord<TPayload>(
        string key,
        int version,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        TPayload payload)
    {
        var metadata = new BusinessConfigurationMetadata(
            DefinitionId: key,
            Name: key,
            Version: version,
            Status: BusinessConfigurationStatus.Active,
            Description: "CAP-020 governed test configuration",
            EffectiveFromUtc: effectiveFromUtc,
            EffectiveToUtc: effectiveToUtc,
            CreatedBy: "cap-020-test",
            ModifiedBy: "cap-020-test",
            CreatedAtUtc: effectiveFromUtc,
            ModifiedAtUtc: effectiveFromUtc,
            AuditMetadata: new Dictionary<string, string>());
        var asset = new BusinessConfigurationAsset<TPayload>(metadata, payload);

        return new ConfigurationRecord(
            new ConfigurationId(Guid.NewGuid()),
            new ConfigurationKey(key),
            ConfigurationScope.Module("subsidyoptimization"),
            new ConfigurationVersion(version),
            new ConfigurationValue(JsonSerializer.Serialize(asset)),
            new EffectivePeriod(effectiveFromUtc, effectiveToUtc),
            "cap-020-test",
            "CAP-020 governed test configuration",
            effectiveFromUtc);
    }
}
