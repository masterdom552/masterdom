using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationExecutionEvidence : ValueObject
{
    private OptimizationExecutionEvidence(
        string? tenantId,
        string? propertyId,
        string configurationContextVersion,
        DateTime effectiveDateUtc,
        decimal occupancyRate,
        decimal confidenceThreshold,
        string algorithmVersion,
        IReadOnlyList<OptimizationMeterInput> meterInputs,
        IReadOnlyList<OptimizationRatingInput> ratingInputs,
        IReadOnlyList<OptimizationImportedDatasetInput> importedDatasets,
        OptimizationPolicySnapshot policy,
        OptimizationModelSnapshot model,
        OptimizationStrategySnapshot strategy,
        IReadOnlyList<OptimizationScenarioEvidence> scenarios,
        string selectedScenarioCode,
        OptimizationOutcomeEvidence outcome)
    {
        TenantId = tenantId;
        PropertyId = propertyId;
        ConfigurationContextVersion = configurationContextVersion;
        EffectiveDateUtc = effectiveDateUtc;
        OccupancyRate = occupancyRate;
        ConfidenceThreshold = confidenceThreshold;
        AlgorithmVersion = algorithmVersion;
        MeterInputs = meterInputs;
        RatingInputs = ratingInputs;
        ImportedDatasets = importedDatasets;
        Policy = policy;
        Model = model;
        Strategy = strategy;
        Scenarios = scenarios;
        SelectedScenarioCode = selectedScenarioCode;
        Outcome = outcome;
    }

    public string? TenantId { get; }
    public string? PropertyId { get; }
    public string ConfigurationContextVersion { get; }
    public DateTime EffectiveDateUtc { get; }
    public decimal OccupancyRate { get; }
    public decimal ConfidenceThreshold { get; }
    public string AlgorithmVersion { get; }
    public IReadOnlyList<OptimizationMeterInput> MeterInputs { get; }
    public IReadOnlyList<OptimizationRatingInput> RatingInputs { get; }
    public IReadOnlyList<OptimizationImportedDatasetInput> ImportedDatasets { get; }
    public OptimizationPolicySnapshot Policy { get; }
    public OptimizationModelSnapshot Model { get; }
    public OptimizationStrategySnapshot Strategy { get; }
    public IReadOnlyList<OptimizationScenarioEvidence> Scenarios { get; }
    public string SelectedScenarioCode { get; }
    public OptimizationOutcomeEvidence Outcome { get; }

    public static OptimizationExecutionEvidence Create(
        string? tenantId,
        string? propertyId,
        string configurationContextVersion,
        DateTime effectiveDateUtc,
        decimal occupancyRate,
        decimal confidenceThreshold,
        string algorithmVersion,
        IReadOnlyList<OptimizationMeterInput> meterInputs,
        IReadOnlyList<OptimizationRatingInput> ratingInputs,
        IReadOnlyList<OptimizationImportedDatasetInput> importedDatasets,
        OptimizationPolicySnapshot policy,
        OptimizationModelSnapshot model,
        OptimizationStrategySnapshot strategy,
        IReadOnlyList<OptimizationScenarioEvidence> scenarios,
        string selectedScenarioCode,
        OptimizationOutcomeEvidence outcome)
    {
        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Optimization evidence effective date must be UTC.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(configurationContextVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(algorithmVersion);
        ArgumentNullException.ThrowIfNull(meterInputs);
        ArgumentNullException.ThrowIfNull(ratingInputs);
        ArgumentNullException.ThrowIfNull(importedDatasets);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedScenarioCode);
        ArgumentNullException.ThrowIfNull(outcome);

        if (meterInputs.Count == 0 || meterInputs.Any(x =>
            x.SanctionedLoad <= 0m
            || string.IsNullOrWhiteSpace(x.MeterType)
            || !string.Equals(x.MeterStatus, "Active", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Optimization evidence requires valid meter inputs with positive sanctioned loads.");
        }

        ValidateConfigurationSnapshot(policy.Identity, policy.Code, "subsidyoptimization.catalog.policy", configurationContextVersion, tenantId, propertyId, effectiveDateUtc);
        ValidateConfigurationSnapshot(model.Identity, model.Code, "subsidyoptimization.catalog.optimization-model", configurationContextVersion, tenantId, propertyId, effectiveDateUtc);
        ValidateConfigurationSnapshot(strategy.Identity, strategy.Code, "subsidyoptimization.catalog.optimization-strategy", configurationContextVersion, tenantId, propertyId, effectiveDateUtc);

        var participatingMeterIds = meterInputs.Select(x => x.MeterId).ToHashSet();
        if (ratingInputs.Any(x => !participatingMeterIds.Contains(x.MeterId)))
        {
            throw new InvalidOperationException("Optimization evidence ratings must correlate to active participating meters.");
        }

        if (scenarios.Count == 0 || scenarios.All(x => !x.IsFeasible))
        {
            throw new InvalidOperationException("Optimization evidence requires at least one feasible scenario.");
        }

        if (!scenarios.Any(x => x.IsFeasible && string.Equals(x.ScenarioCode, selectedScenarioCode, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException("Selected scenario must identify a feasible evaluated scenario.");
        }

        foreach (var scenario in scenarios.Where(x => x.IsFeasible))
        {
            ValidateFeasibleScenario(scenario, model, strategy, participatingMeterIds);
        }

        var selectedScenario = scenarios.Single(x =>
            x.IsFeasible && string.Equals(x.ScenarioCode, selectedScenarioCode, StringComparison.Ordinal));
        if (outcome.EstimatedSavings != decimal.Round(selectedScenario.ExpectedSubsidy, 2, MidpointRounding.AwayFromZero)
            || outcome.EstimatedCost != decimal.Round(selectedScenario.ExpectedCost, 2, MidpointRounding.AwayFromZero)
            || !string.Equals(outcome.Summary, selectedScenario.TradeOffSummary, StringComparison.Ordinal)
            || outcome.RecommendationId == Guid.Empty
            || string.IsNullOrWhiteSpace(outcome.RecommendationTitle)
            || string.IsNullOrWhiteSpace(outcome.RecommendationDetails)
            || string.IsNullOrWhiteSpace(outcome.RecommendationPriority))
        {
            throw new InvalidOperationException("Optimization outcome and recommendation must be consistent with the selected scenario.");
        }

        return new OptimizationExecutionEvidence(
            tenantId?.Trim(),
            propertyId?.Trim(),
            configurationContextVersion.Trim(),
            effectiveDateUtc,
            occupancyRate,
            confidenceThreshold,
            algorithmVersion.Trim(),
            meterInputs.ToArray(),
            ratingInputs.ToArray(),
            importedDatasets.ToArray(),
            policy,
            model,
            strategy,
            scenarios.ToArray(),
            selectedScenarioCode.Trim(),
            outcome);
    }

    private static void ValidateConfigurationSnapshot(
        OptimizationConfigurationIdentity identity,
        string code,
        string expectedConfigurationKey,
        string configurationContextVersion,
        string? tenantId,
        string? propertyId,
        DateTime effectiveDateUtc)
    {
        if (string.IsNullOrWhiteSpace(code)
            || string.IsNullOrWhiteSpace(configurationContextVersion)
            || !string.Equals(identity.ConfigurationKey, expectedConfigurationKey, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(identity.DefinitionId)
            || identity.Version <= 0
            || identity.EffectiveFromUtc.Kind != DateTimeKind.Utc
            || identity.EffectiveToUtc?.Kind is not (null or DateTimeKind.Utc)
            || effectiveDateUtc < identity.EffectiveFromUtc
            || effectiveDateUtc >= identity.EffectiveToUtc
            || !string.Equals(identity.TenantId, tenantId, StringComparison.Ordinal)
            || !string.Equals(identity.PropertyId, propertyId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Optimization evidence requires a valid governed configuration identity and scope.");
        }
    }

    private static void ValidateFeasibleScenario(
        OptimizationScenarioEvidence scenario,
        OptimizationModelSnapshot model,
        OptimizationStrategySnapshot strategy,
        IReadOnlySet<Guid> participatingMeterIds)
    {
        if (string.IsNullOrWhiteSpace(scenario.ScenarioCode)
            || scenario.ConsumptionUnits < 0m
            || scenario.MeterAllocations.Count == 0
            || scenario.MeterAllocations.Any(x =>
                x.MeterId == Guid.Empty
                || x.BaselineUnits < 0m
                || x.AllocatedUnits < 0m
                || x.SanctionedLoad <= 0m
                || x.AllocatedUnits > x.SanctionedLoad)
            || scenario.MeterAllocations.Select(x => x.MeterId).Distinct().Count() != scenario.MeterAllocations.Count
            || !scenario.MeterAllocations.Select(x => x.MeterId).ToHashSet().SetEquals(participatingMeterIds)
            || !OptimizationAllocationInvariant.IsConserved(
                scenario.ConsumptionUnits,
                scenario.MeterAllocations.Select(x => x.AllocatedUnits),
                model.BoundaryTolerance)
            || !OptimizationAllocationInvariant.IsMovementConserved(
                scenario.MeterAllocations.Select(x => x.MovementUnits),
                model.BoundaryTolerance)
            || !OptimizationAllocationInvariant.IsWithinMovementBudget(
                scenario.ConsumptionUnits,
                strategy.MaximumCrossMeterMovementFraction,
                scenario.MeterAllocations.Select(x => x.MovementUnits),
                model.BoundaryTolerance)
            || (!strategy.PermitCrossMeterMovement && scenario.MeterAllocations.Any(x => x.MovementUnits != 0m)))
        {
            throw new InvalidOperationException("Feasible optimization evidence violates allocation, load, conservation, or movement invariants.");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TenantId;
        yield return PropertyId;
        yield return ConfigurationContextVersion;
        yield return EffectiveDateUtc;
        yield return OccupancyRate;
        yield return ConfidenceThreshold;
        yield return AlgorithmVersion;
        foreach (var input in MeterInputs) yield return input;
        foreach (var input in RatingInputs) yield return input;
        foreach (var dataset in ImportedDatasets) yield return dataset;
        yield return Policy;
        yield return Model;
        yield return Strategy;
        foreach (var scenario in Scenarios) yield return scenario;
        yield return SelectedScenarioCode;
        yield return Outcome;
    }
}

public sealed record OptimizationMeterInput(Guid MeterId, DateOnly PeriodStart, DateOnly PeriodEnd, decimal ConsumptionUnits, DateTime CapturedAtUtc, string MeterType, string MeterStatus, decimal SanctionedLoad);

public sealed record OptimizationRatingInput(Guid RatingId, Guid MeterId, DateOnly PeriodStart, DateOnly PeriodEnd, decimal RatedUnits, decimal RatedAmount, DateTime RatedAtUtc);

public sealed record OptimizationImportedDatasetInput(string DatasetId, string DatasetType, string SourceSystem, string Version, DateTime ImportedAtUtc);

public sealed record OptimizationConfigurationIdentity(string ConfigurationKey, string DefinitionId, int Version, DateTime EffectiveFromUtc, DateTime? EffectiveToUtc, string? TenantId, string? PropertyId);

public sealed record OptimizationSubsidySlabSnapshot(decimal MaximumUnits, decimal SubsidyAmount, bool IsCliff);

public sealed record OptimizationPolicySnapshot(string Code, OptimizationConfigurationIdentity Identity, IReadOnlyList<OptimizationSubsidySlabSnapshot> Slabs, decimal SanctionedLoadLimit, decimal SanctionedLoadPenaltyPerUnit, IReadOnlyList<string> EligibleMeterTypes)
{
    public int Version => Identity.Version;
}

public sealed record OptimizationModelSnapshot(string Code, OptimizationConfigurationIdentity Identity, decimal SubsidyWeight, decimal CostWeight, decimal LoadImpactWeight, decimal RiskWeight, decimal BoundaryTolerance, int MaximumScenarioCount)
{
    public int Version => Identity.Version;
}

public sealed record OptimizationStrategySnapshot(string Code, OptimizationConfigurationIdentity Identity, IReadOnlyList<decimal> ConsumptionFactors, bool IncludeSubsidyBoundaries, bool PermitCrossMeterMovement, decimal MaximumCrossMeterMovementFraction)
{
    public int Version => Identity.Version;
}

public sealed record OptimizationMeterAllocationEvidence(Guid MeterId, decimal BaselineUnits, decimal AllocatedUnits, decimal SanctionedLoad, decimal MovementUnits);

public sealed record OptimizationScenarioEvidence(string ScenarioCode, decimal ConsumptionUnits, decimal ExpectedSubsidy, decimal ExpectedCost, decimal SanctionedLoadImpact, decimal Score, bool IsFeasible, string? InfeasibilityReason, decimal? TriggeredBoundary, string TradeOffSummary, IReadOnlyList<OptimizationMeterAllocationEvidence> MeterAllocations);

public sealed record OptimizationOutcomeEvidence(decimal EstimatedSavings, decimal EstimatedCost, string Summary, Guid RecommendationId, string RecommendationTitle, string RecommendationDetails, string RecommendationPriority);
