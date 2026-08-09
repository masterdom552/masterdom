using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.Events;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Core.Tests.SubsidyOptimization;

public sealed class SubsidyOptimizationDomainTests
{
    [Fact]
    public void Start_ShouldCreateVersionOne_AndRaiseStartedEvent()
    {
        var run = CreateStartedRun();

        Assert.Equal(1, run.OptimizationVersion.Value);
        Assert.Equal(OptimizationStatus.Started, run.OptimizationStatus);
        Assert.Contains(run.DomainEvents, x => x is OptimizationStartedDomainEvent);
    }

    [Fact]
    public void Complete_ShouldCompleteWhenArtifactsMatchExecutionEvidence()
    {
        var run = CreateStartedRun();
        var evidence = CreateExecutionEvidence();

        run.Complete(
            CreateMatchingResult(evidence),
            ConsumptionForecast.Create(200m, 180m, "Deterministic placeholder forecast."),
            CreateMatchingRecommendationSet(evidence),
            DateTime.UtcNow,
            evidence);

        Assert.Equal(OptimizationStatus.Completed, run.OptimizationStatus);
        Assert.NotNull(run.OptimizationResult);
        Assert.NotNull(run.ConsumptionForecast);
        Assert.Single(run.Snapshots);
        Assert.Single(run.Recommendations);
        Assert.Contains(run.DomainEvents, x => x is OptimizationCompletedDomainEvent);
        Assert.Contains(run.DomainEvents, x => x is RecommendationGeneratedDomainEvent);
    }

    [Fact]
    public void Complete_ShouldRejectResultInconsistentWithExecutionEvidence()
    {
        var run = CreateStartedRun();
        var evidence = CreateExecutionEvidence();

        var exception = Assert.Throws<InvalidOperationException>(() => run.Complete(
            OptimizationResult.Create(49m, evidence.Outcome.EstimatedCost, evidence.Outcome.Summary),
            ConsumptionForecast.Create(200m, 180m, "Deterministic placeholder forecast."),
            CreateMatchingRecommendationSet(evidence),
            DateTime.UtcNow,
            evidence));

        Assert.Equal("Optimization result must match the validated execution evidence.", exception.Message);
        Assert.Equal(OptimizationStatus.Started, run.OptimizationStatus);
        Assert.Null(run.OptimizationResult);
    }

    [Fact]
    public void Complete_ShouldRejectRecommendationInconsistentWithExecutionEvidence()
    {
        var run = CreateStartedRun();
        var evidence = CreateExecutionEvidence();
        var mismatchedRecommendation = RecommendationSet.Create(
        [
            OptimizationRecommendation.Generate(
                RecommendationId.New(),
                evidence.Outcome.RecommendationTitle,
                evidence.Outcome.RecommendationDetails,
                RecommendationPriority.Create(evidence.Outcome.RecommendationPriority),
                DateTime.UtcNow)
        ]);

        var exception = Assert.Throws<InvalidOperationException>(() => run.Complete(
            CreateMatchingResult(evidence),
            ConsumptionForecast.Create(200m, 180m, "Deterministic placeholder forecast."),
            mismatchedRecommendation,
            DateTime.UtcNow,
            evidence));

        Assert.Equal("Optimization recommendation must match the validated execution evidence.", exception.Message);
        Assert.Equal(OptimizationStatus.Started, run.OptimizationStatus);
        Assert.Empty(run.Recommendations);
    }

    [Fact]
    public void Complete_ShouldRejectSecondCompletion_AsImmutableHistory()
    {
        var run = CreateCompletedRun();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            run.Complete(
                OptimizationResult.Create(200m, 200m, "Should fail"),
                ConsumptionForecast.Create(200m, 180m, "Should fail"),
                RecommendationSet.Create(
                [
                    OptimizationRecommendation.Generate(
                        RecommendationId.New(),
                        "Invalid second completion",
                        "This should not execute.",
                        RecommendationPriority.Low,
                        DateTime.UtcNow)
                ]),
                DateTime.UtcNow,
                CreateExecutionEvidence()));

        Assert.Equal("Completed optimization runs are immutable.", exception.Message);
    }

    [Fact]
    public void CreateScenarioVersion_ShouldCreateNewRunWithIncrementedVersion()
    {
        var run = CreateCompletedRun();

        var next = run.CreateScenarioVersion(
            DateTime.UtcNow,
            RatingReference.Create([Guid.NewGuid()]));

        Assert.Equal(run.OptimizationVersion.Value + 1, next.OptimizationVersion.Value);
        Assert.NotEqual(run.Id, next.Id);
        Assert.Contains(next.DomainEvents, x => x is ScenarioVersionCreatedDomainEvent);
    }

    [Fact]
    public void RecommendationSet_ShouldRejectDuplicateRecommendationIds()
    {
        var recommendationId = RecommendationId.New();

        var duplicateA = OptimizationRecommendation.Generate(
            recommendationId,
            "Duplicate",
            "A",
            RecommendationPriority.High,
            DateTime.UtcNow);

        var duplicateB = OptimizationRecommendation.Generate(
            recommendationId,
            "Duplicate",
            "B",
            RecommendationPriority.Low,
            DateTime.UtcNow);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            RecommendationSet.Create([duplicateA, duplicateB]));

        Assert.Equal("Duplicate recommendation identifiers are not allowed.", exception.Message);
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectNonconservedFeasibleAllocation()
    {
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            meterId => CreateScenario(meterId, 100m, 99m, 120m, 0m)));
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectNegativeFeasibleAllocation()
    {
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            meterId => CreateScenario(meterId, -1m, -1m, 120m, 0m)));
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectPerMeterSanctionedLoadViolation()
    {
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            meterId => CreateScenario(meterId, 121m, 121m, 120m, 0m)));
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectMovementAboveGovernedBudget()
    {
        var secondMeterId = Guid.NewGuid();
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            meterId => new OptimizationScenarioEvidence(
                "candidate-1",
                100m,
                50m,
                0m,
                30m,
                50m,
                true,
                null,
                100m,
                "valid",
                [
                    new OptimizationMeterAllocationEvidence(meterId, 50m, 20m, 120m, -30m),
                    new OptimizationMeterAllocationEvidence(secondMeterId, 50m, 80m, 120m, 30m)
                ]),
            permitMovement: true,
            maximumMovementFraction: 0.2m,
            additionalMeterId: secondMeterId));
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectWrongConfigurationCatalogIdentity()
    {
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            policyConfigurationKey: "subsidyoptimization.catalog.optimization-model"));
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectRatingForNonparticipatingMeter()
    {
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            ratingFactory: _ =>
            [
                new OptimizationRatingInput(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 100m, 50m, DateTime.UtcNow)
            ]));
    }

    [Fact]
    public void ExecutionEvidence_ShouldRejectOutcomeInconsistentWithSelectedScenario()
    {
        Assert.Throws<InvalidOperationException>(() => CreateExecutionEvidence(
            outcomeFactory: _ => new OptimizationOutcomeEvidence(999m, 0m, "valid", Guid.NewGuid(), "Select candidate", "valid", "High")));
    }

    private static OptimizationRunAggregate CreateStartedRun()
    {
        return OptimizationRunAggregate.Start(
            OptimizationRunId.New(),
            SubsidyScenario.Create(
                ScenarioId.Create("SCN-BASELINE"),
                "Baseline Scenario",
                "Generic baseline advisory scenario."),
            MeterGroup.Create(
                MeterGroupReference.Create("GRP-A", [Guid.NewGuid(), Guid.NewGuid()]),
                "Sample Group"),
            RatingReference.Create([Guid.NewGuid()]),
            OptimizationPeriod.Create(
                DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30)),
                DateOnly.FromDateTime(DateTime.UtcNow.Date)),
            DateTime.UtcNow);
    }

    private static OptimizationRunAggregate CreateCompletedRun()
    {
        var run = CreateStartedRun();
        var evidence = CreateExecutionEvidence();

        run.Complete(
            CreateMatchingResult(evidence),
            ConsumptionForecast.Create(190m, 175m, "Deterministic baseline"),
            CreateMatchingRecommendationSet(evidence),
            DateTime.UtcNow,
            evidence);

        return run;
    }

    private static OptimizationResult CreateMatchingResult(OptimizationExecutionEvidence evidence)
    {
        return OptimizationResult.Create(
            evidence.Outcome.EstimatedSavings,
            evidence.Outcome.EstimatedCost,
            evidence.Outcome.Summary);
    }

    private static RecommendationSet CreateMatchingRecommendationSet(OptimizationExecutionEvidence evidence)
    {
        return RecommendationSet.Create(
        [
            OptimizationRecommendation.Generate(
                RecommendationId.From(evidence.Outcome.RecommendationId),
                evidence.Outcome.RecommendationTitle,
                evidence.Outcome.RecommendationDetails,
                RecommendationPriority.Create(evidence.Outcome.RecommendationPriority),
                DateTime.UtcNow)
        ]);
    }

    private static OptimizationExecutionEvidence CreateExecutionEvidence(
        Func<Guid, OptimizationScenarioEvidence>? scenarioFactory = null,
        Func<Guid, IReadOnlyList<OptimizationRatingInput>>? ratingFactory = null,
        string policyConfigurationKey = "subsidyoptimization.catalog.policy",
        bool permitMovement = false,
        decimal maximumMovementFraction = 0m,
        Guid? additionalMeterId = null,
        Func<OptimizationScenarioEvidence, OptimizationOutcomeEvidence>? outcomeFactory = null)
    {
        var meterId = Guid.NewGuid();
        static OptimizationConfigurationIdentity CreateIdentity(string key) => new(
            key,
            "record-test",
            1,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            null,
            "tenant-1",
            "property-1");
        var scenario = scenarioFactory?.Invoke(meterId)
            ?? CreateScenario(meterId, 100m, 100m, 120m, 0m);
        var meterIds = additionalMeterId.HasValue ? new[] { meterId, additionalMeterId.Value } : [meterId];
        var outcome = outcomeFactory?.Invoke(scenario)
            ?? new OptimizationOutcomeEvidence(50m, 0m, "valid", Guid.NewGuid(), "Select candidate", "valid", "High");
        return OptimizationExecutionEvidence.Create(
            "tenant-1",
            "property-1",
            "context-v1",
            new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
            1m,
            0.5m,
            "subsidy-optimizer-v1",
            meterIds.Select(id => new OptimizationMeterInput(id, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 100m, DateTime.UtcNow, "residential", "Active", 120m)).ToArray(),
            ratingFactory?.Invoke(meterId) ?? [],
            [],
            new OptimizationPolicySnapshot("policy", CreateIdentity(policyConfigurationKey), [new OptimizationSubsidySlabSnapshot(100m, 50m, true)], 120m, 1m, ["residential"]),
            new OptimizationModelSnapshot("model", CreateIdentity("subsidyoptimization.catalog.optimization-model"), 1m, 1m, 1m, 1m, 0.01m, 3),
            new OptimizationStrategySnapshot("strategy", CreateIdentity("subsidyoptimization.catalog.optimization-strategy"), [1m], false, permitMovement, maximumMovementFraction),
            [scenario],
            "candidate-1",
            outcome);
    }

    private static OptimizationScenarioEvidence CreateScenario(
        Guid meterId,
        decimal consumptionUnits,
        decimal allocatedUnits,
        decimal sanctionedLoad,
        decimal movementUnits)
    {
        return new OptimizationScenarioEvidence(
            "candidate-1",
            consumptionUnits,
            50m,
            0m,
            decimal.Max(0m, movementUnits),
            50m,
            true,
            null,
            100m,
            "valid",
            [new OptimizationMeterAllocationEvidence(meterId, consumptionUnits, allocatedUnits, sanctionedLoad, movementUnits)]);
    }
}
