namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyPolicyConfiguration(
    string PolicyCode,
    IReadOnlyList<SubsidySlabConfiguration> Slabs,
    decimal SanctionedLoadLimit,
    decimal SanctionedLoadPenaltyPerUnit,
    IReadOnlyList<string> EligibleMeterTypes);

public sealed record SubsidySlabConfiguration(
    decimal MaximumUnits,
    decimal SubsidyAmount,
    bool IsCliff);

public sealed record OptimizationModelConfiguration(
    string ModelCode,
    decimal SubsidyWeight,
    decimal CostWeight,
    decimal LoadImpactWeight,
    decimal RiskWeight,
    decimal BoundaryTolerance,
    int MaximumScenarioCount);

public sealed record OptimizationStrategyConfiguration(
    string StrategyCode,
    IReadOnlyList<decimal> ConsumptionFactors,
    bool IncludeSubsidyBoundaries,
    bool PermitCrossMeterMovement,
    decimal MaximumCrossMeterMovementFraction);

public sealed record ResolvedConfigurationIdentity(
    string ConfigurationKey,
    string DefinitionId,
    int Version,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string? TenantId,
    string? PropertyId);

public sealed record ResolvedSubsidyOptimizerConfiguration(
    SubsidyPolicyConfiguration Policy,
    OptimizationModelConfiguration Model,
    OptimizationStrategyConfiguration Strategy,
    IReadOnlyDictionary<string, string> Versions,
    ResolvedConfigurationIdentity PolicyIdentity,
    ResolvedConfigurationIdentity ModelIdentity,
    ResolvedConfigurationIdentity StrategyIdentity);
