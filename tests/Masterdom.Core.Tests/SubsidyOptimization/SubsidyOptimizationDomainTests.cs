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
    public void Complete_ShouldCreateSnapshotAndRecommendationEvents()
    {
        var run = CreateStartedRun();

        run.Complete(
            OptimizationResult.Create(100m, 250m, "Advisory optimization completed."),
            ConsumptionForecast.Create(200m, 180m, "Deterministic placeholder forecast."),
            RecommendationSet.Create(
            [
                OptimizationRecommendation.Generate(
                    RecommendationId.New(),
                    "Validate eligibility windows",
                    "Review configured eligibility windows against the latest consumption profile.",
                    RecommendationPriority.High,
                    DateTime.UtcNow)
            ]),
            DateTime.UtcNow);

        Assert.Equal(OptimizationStatus.Completed, run.OptimizationStatus);
        Assert.NotNull(run.OptimizationResult);
        Assert.NotNull(run.ConsumptionForecast);
        Assert.Single(run.Snapshots);
        Assert.Single(run.Recommendations);
        Assert.Contains(run.DomainEvents, x => x is OptimizationCompletedDomainEvent);
        Assert.Contains(run.DomainEvents, x => x is RecommendationGeneratedDomainEvent);
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
                DateTime.UtcNow));

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

        run.Complete(
            OptimizationResult.Create(50m, 175m, "Completed"),
            ConsumptionForecast.Create(190m, 175m, "Deterministic baseline"),
            RecommendationSet.Create(
            [
                OptimizationRecommendation.Generate(
                    RecommendationId.New(),
                    "Review enrollment timeline",
                    "Check enrollment lead times for future billing cycle optimization.",
                    RecommendationPriority.Medium,
                    DateTime.UtcNow)
            ]),
            DateTime.UtcNow);

        return run;
    }
}
