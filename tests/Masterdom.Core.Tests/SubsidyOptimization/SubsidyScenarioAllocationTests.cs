using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Platform.CalculationEngine;
using Masterdom.Platform.CalculationEngine.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Core.Tests.SubsidyOptimization;

public sealed class SubsidyScenarioAllocationTests
{
    [Fact]
    public void Generate_ShouldKeepMetersInProportionalAllocation_WhenMovementIsDisabled()
    {
        var scenario = Generate(CreateStrategy(permitMovement: false), CreateModel())[0];

        Assert.All(scenario.MeterAllocations, allocation => Assert.Equal(0m, allocation.MovementUnits));
        Assert.Equal(80m, scenario.MeterAllocations[0].AllocatedUnits);
        Assert.Equal(20m, scenario.MeterAllocations[1].AllocatedUnits);
    }

    [Fact]
    public void Generate_ShouldRedistributeWithinGovernedBoundary_WhenMovementIsEnabled()
    {
        var scenario = Generate(CreateStrategy(permitMovement: true), CreateModel())[0];

        Assert.Equal(50m, scenario.MeterAllocations[0].AllocatedUnits);
        Assert.Equal(50m, scenario.MeterAllocations[1].AllocatedUnits);
        Assert.Equal(-30m, scenario.MeterAllocations[0].MovementUnits);
        Assert.Equal(30m, scenario.MeterAllocations[1].MovementUnits);
    }

    [Fact]
    public void RankScenarios_ShouldRejectIndividualMeterSanctionedLoadViolation()
    {
        var strategy = CreateStrategy(permitMovement: false);
        var ranked = Evaluate(Generate(strategy, CreateModel()), strategy);

        Assert.False(ranked[0].IsFeasible);
        Assert.True(ranked[0].SanctionedLoadImpact > 0m);
    }

    [Fact]
    public void Generate_ShouldConservePropertyConsumptionAcrossMeterAllocations()
    {
        var scenarios = Generate(CreateStrategy(permitMovement: true), CreateModel());

        Assert.All(scenarios, scenario =>
            Assert.Equal(scenario.ForecastConsumptionUnits, scenario.MeterAllocations.Sum(x => x.AllocatedUnits)));
    }

    [Fact]
    public void RankScenarios_ShouldAcceptValidGovernedRedistribution()
    {
        var strategy = CreateStrategy(permitMovement: true);
        var ranked = Evaluate(Generate(strategy, CreateModel()), strategy);

        Assert.True(ranked[0].IsFeasible);
        Assert.All(ranked[0].MeterAllocations, allocation => Assert.True(allocation.AllocatedUnits <= allocation.SanctionedLoad));
    }

    [Fact]
    public void Generate_ShouldRetainAllMandatoryCliffsBeforeBoundingOptionalCandidates()
    {
        var policy = CreatePolicy() with
        {
            Slabs =
            [
                new SubsidySlabConfiguration(100m, 100m, true),
                new SubsidySlabConfiguration(200m, 50m, true),
                new SubsidySlabConfiguration(decimal.MaxValue, 0m, false)
            ]
        };
        var scenarios = new ScenarioGenerator().Generate(
            CreateEstimate(),
            new SubsidyForecast(150m, 1m, 0m),
            policy,
            CreateStrategy(permitMovement: false) with { IncludeSubsidyBoundaries = true },
            CreateModel() with { MaximumScenarioCount = 6 });

        Assert.Equal(6, scenarios.Count);
        Assert.Equal([99.99m, 100m, 100.01m, 199.99m, 200m, 200.01m], scenarios.Select(x => x.ForecastConsumptionUnits));
        Assert.DoesNotContain(scenarios, x => x.ForecastConsumptionUnits == 150m);
    }

    [Fact]
    public void Generate_ShouldShareExactMovementBudgetAcrossMultipleDonors()
    {
        var estimate = new SubsidyConsumptionEstimate(
            120m,
            120m,
            120m,
            120m,
            1m,
            [
                new SubsidyMeterEstimate(Guid.Parse("00000000-0000-0000-0000-000000000001"), 60m, 40m),
                new SubsidyMeterEstimate(Guid.Parse("00000000-0000-0000-0000-000000000002"), 60m, 40m),
                new SubsidyMeterEstimate(Guid.Parse("00000000-0000-0000-0000-000000000003"), 0m, 100m)
            ]);
        var strategy = CreateStrategy(permitMovement: true, maximumMovementFraction: 0.25m);

        var scenario = new ScenarioGenerator().Generate(
            estimate,
            new SubsidyForecast(120m, 1m, 0m),
            CreatePolicy(),
            strategy,
            CreateModel())[0];

        Assert.Equal(30m, scenario.MeterAllocations.Where(x => x.MovementUnits > 0m).Sum(x => x.MovementUnits));
        Assert.Equal(0m, scenario.MeterAllocations.Sum(x => x.MovementUnits));
    }

    [Fact]
    public void RankScenarios_ShouldRejectExternallySuppliedMovementAbovePropertyBudget()
    {
        var scenario = CreateScenario(
            "over-budget",
            120m,
            [
                new SubsidyMeterAllocation(Guid.NewGuid(), 40m, 20m, 100m, -20m),
                new SubsidyMeterAllocation(Guid.NewGuid(), 40m, 20m, 100m, -20m),
                new SubsidyMeterAllocation(Guid.NewGuid(), 40m, 80m, 100m, 40m)
            ]);

        var ranked = Evaluate(
            [scenario],
            CreateStrategy(permitMovement: true, maximumMovementFraction: 0.25m));

        Assert.False(ranked[0].IsFeasible);
    }

    [Fact]
    public void Generate_ShouldRetainMandatoryCliffs_WhenStrategySuppressesBoundaryCandidates()
    {
        var policy = CreatePolicy() with
        {
            Slabs =
            [
                new SubsidySlabConfiguration(100m, 100m, true),
                new SubsidySlabConfiguration(decimal.MaxValue, 0m, false)
            ]
        };

        var scenarios = new ScenarioGenerator().Generate(
            CreateEstimate(),
            new SubsidyForecast(150m, 1m, 0m),
            policy,
            CreateStrategy(permitMovement: false) with { IncludeSubsidyBoundaries = false },
            CreateModel() with { MaximumScenarioCount = 3 });

        Assert.Equal([99.99m, 100m, 100.01m], scenarios.Select(x => x.ForecastConsumptionUnits));
    }

    [Fact]
    public void RankScenarios_ShouldChangeSelection_WhenCostWeightChanges()
    {
        Assert.Equal("high-subsidy", RankTradeOff(costWeight: 0m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 3m, ratedAmount: 0m));
        Assert.Equal("low-impact", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 3m, ratedAmount: 0m));
    }

    [Fact]
    public void RankScenarios_ShouldChangeSelection_WhenLoadImpactWeightChanges()
    {
        Assert.Equal("high-subsidy", RankTradeOff(costWeight: 0m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 0m, ratedAmount: 0m));
        Assert.Equal("low-impact", RankTradeOff(costWeight: 0m, loadImpactWeight: 3m, subsidyWeight: 1m, penalty: 0m, ratedAmount: 0m));
    }

    [Fact]
    public void RankScenarios_ShouldChangeSelection_WhenPenaltyChanges()
    {
        Assert.Equal("high-subsidy", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 0m, ratedAmount: 0m));
        Assert.Equal("low-impact", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 3m, ratedAmount: 0m));
    }

    [Fact]
    public void RankScenarios_ShouldChangeSelection_WhenRatedAmountChanges()
    {
        Assert.Equal("low-impact", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 3m, ratedAmount: 0m));
        Assert.Equal("high-subsidy", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 3m, ratedAmount: 100m));
    }

    [Fact]
    public void RankScenarios_ShouldChangeSelection_WhenSubsidyWeightChanges()
    {
        Assert.Equal("low-impact", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 1m, penalty: 3m, ratedAmount: 0m));
        Assert.Equal("high-subsidy", RankTradeOff(costWeight: 1m, loadImpactWeight: 0m, subsidyWeight: 2m, penalty: 3m, ratedAmount: 0m));
    }

    private static IReadOnlyList<SubsidyOptimizationScenario> Generate(
        OptimizationStrategyConfiguration strategy,
        OptimizationModelConfiguration model)
    {
        return new ScenarioGenerator().Generate(
            CreateEstimate(),
            new SubsidyForecast(100m, 1m, 0m),
            CreatePolicy(),
            strategy,
            model);
    }

    private static IReadOnlyList<SubsidyOptimizationScenario> Evaluate(
        IReadOnlyCollection<SubsidyOptimizationScenario> scenarios,
        OptimizationStrategyConfiguration strategy,
        SubsidyPolicyConfiguration? policy = null,
        OptimizationModelConfiguration? model = null,
        decimal ratedAmount = 25m)
    {
        var services = new ServiceCollection();
        services.AddCalculationEngine();
        using var provider = services.BuildServiceProvider();
        var runtime = new SubsidyCalculationRuntimeInvoker(provider.GetRequiredService<ICalculationRuntime>());

        return new ScenarioEvaluator(runtime).RankScenarios(
            scenarios,
            policy ?? CreatePolicy(),
            model ?? CreateModel(),
            strategy,
            [new RatedConsumptionContract(
                Guid.NewGuid(),
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 31),
                100m,
                ratedAmount,
                new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc))],
            new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc));
    }

    private static string RankTradeOff(
        decimal costWeight,
        decimal loadImpactWeight,
        decimal subsidyWeight,
        decimal penalty,
        decimal ratedAmount)
    {
        var highSubsidy = CreateScenario(
            "high-subsidy",
            100m,
            [
                new SubsidyMeterAllocation(Guid.NewGuid(), 70m, 50m, 100m, -20m),
                new SubsidyMeterAllocation(Guid.NewGuid(), 30m, 50m, 100m, 20m)
            ]);
        var lowImpact = CreateScenario(
            "low-impact",
            120m,
            [
                new SubsidyMeterAllocation(Guid.NewGuid(), 60m, 60m, 100m, 0m),
                new SubsidyMeterAllocation(Guid.NewGuid(), 60m, 60m, 100m, 0m)
            ]);
        var policy = new SubsidyPolicyConfiguration(
            "trade-off-policy",
            [
                new SubsidySlabConfiguration(100m, 100m, false),
                new SubsidySlabConfiguration(decimal.MaxValue, 50m, false)
            ],
            150m,
            penalty,
            ["residential"]);
        var model = new OptimizationModelConfiguration(
            "trade-off-model",
            subsidyWeight,
            costWeight,
            loadImpactWeight,
            0m,
            0.01m,
            2);

        return Evaluate(
            [highSubsidy, lowImpact],
            CreateStrategy(permitMovement: true, maximumMovementFraction: 0.5m),
            policy,
            model,
            ratedAmount)[0].ScenarioCode;
    }

    private static SubsidyOptimizationScenario CreateScenario(
        string code,
        decimal units,
        IReadOnlyList<SubsidyMeterAllocation> allocations)
    {
        return new SubsidyOptimizationScenario(
            code,
            code,
            units,
            units,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            0m,
            true,
            null,
            null,
            string.Empty,
            0m,
            allocations);
    }

    private static SubsidyConsumptionEstimate CreateEstimate()
    {
        return new SubsidyConsumptionEstimate(
            100m,
            100m,
            100m,
            100m,
            1m,
            [
                new SubsidyMeterEstimate(Guid.Parse("00000000-0000-0000-0000-000000000001"), 80m, 50m),
                new SubsidyMeterEstimate(Guid.Parse("00000000-0000-0000-0000-000000000002"), 20m, 100m)
            ]);
    }

    private static SubsidyPolicyConfiguration CreatePolicy()
    {
        return new SubsidyPolicyConfiguration(
            "allocation-policy",
            [new SubsidySlabConfiguration(200m, 100m, false)],
            200m,
            1m,
            ["residential"]);
    }

    private static OptimizationModelConfiguration CreateModel()
    {
        return new OptimizationModelConfiguration("allocation-model", 1m, 1m, 1m, 1m, 0.01m, 10);
    }

    private static OptimizationStrategyConfiguration CreateStrategy(
        bool permitMovement,
        decimal? maximumMovementFraction = null)
    {
        return new OptimizationStrategyConfiguration(
            "allocation-strategy",
            [1m],
            false,
            permitMovement,
            permitMovement ? maximumMovementFraction ?? 1m : 0m);
    }
}
