namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed class ScenarioGenerator
{
    public IReadOnlyList<SubsidyOptimizationScenario> Generate(
        SubsidyConsumptionEstimate estimate,
        SubsidyForecast forecast)
    {
        ArgumentNullException.ThrowIfNull(estimate);
        ArgumentNullException.ThrowIfNull(forecast);

        var baseline = BuildScenario(
            scenarioCode: "baseline",
            scenarioName: "Baseline Stability",
            estimated: estimate.OccupancyAdjustedUnits,
            projected: forecast.ProjectedConsumptionUnits,
            benefitMultiplier: 0.04m,
            riskMultiplier: 0.02m);

        var preserve = BuildScenario(
            scenarioCode: "preserve",
            scenarioName: "Subsidy Preservation",
            estimated: estimate.OccupancyAdjustedUnits * 0.98m,
            projected: forecast.ProjectedConsumptionUnits * 0.95m,
            benefitMultiplier: 0.08m,
            riskMultiplier: 0.03m);

        var optimize = BuildScenario(
            scenarioCode: "optimize",
            scenarioName: "Optimization Push",
            estimated: estimate.OccupancyAdjustedUnits * 0.94m,
            projected: forecast.ProjectedConsumptionUnits * 0.90m,
            benefitMultiplier: 0.11m,
            riskMultiplier: 0.07m);

        return [baseline, preserve, optimize];
    }

    private static SubsidyOptimizationScenario BuildScenario(
        string scenarioCode,
        string scenarioName,
        decimal estimated,
        decimal projected,
        decimal benefitMultiplier,
        decimal riskMultiplier)
    {
        var expectedBenefit = Math.Max(estimated - projected, 0m) * benefitMultiplier;
        var expectedRisk = Math.Abs(projected - estimated) * riskMultiplier;
        var thresholdDelta = estimated - projected;
        var preservation = expectedBenefit == 0m
            ? 0.5m
            : Math.Clamp((expectedBenefit - expectedRisk) / expectedBenefit, 0m, 1m);

        return new SubsidyOptimizationScenario(
            ScenarioCode: scenarioCode,
            ScenarioName: scenarioName,
            EstimatedConsumptionUnits: estimated,
            ForecastConsumptionUnits: projected,
            ExpectedBenefit: expectedBenefit,
            ExpectedRisk: expectedRisk,
            ThresholdDelta: thresholdDelta,
            SubsidyPreservationScore: preservation,
            TradeOffSummary: $"benefit={expectedBenefit:F2};risk={expectedRisk:F2}",
            RankScore: 0m);
    }
}
