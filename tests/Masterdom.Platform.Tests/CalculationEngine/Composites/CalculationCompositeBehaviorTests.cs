using Masterdom.Platform.CalculationEngine.Composites;
using Masterdom.Platform.CalculationEngine.Contracts;

namespace Masterdom.Platform.Tests.CalculationEngine.Composites;

public sealed class CalculationCompositeBehaviorTests
{
    private static readonly CalculationContext DefaultContext = new(
        new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void ConsumptionEstimationComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new ConsumptionEstimationCompositeCalculator();

        var input = new ConsumptionEstimationCompositeInput(
            historicalValues: [100m, 110m, 90m],
            historicalWeights: [1m, 2m, 1m],
            blendWeight: 0.25m,
            occupancyNumerator: 9m,
            occupancyDenominator: 10m,
            completenessObservedCount: 8m,
            completenessExpectedCount: 10m,
            clampMin: 0m,
            clampMax: 200m);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal(100m, output.EstimatedBaseline);
        Assert.Equal(100.625m, output.FailedMeterEstimate);
        Assert.Equal(90.5625m, output.OccupancyAdjustedEstimate);
        Assert.Equal(0.8m, output.DataCompletenessRatio);
    }

    [Fact]
    public void ForecastProjectionComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new ForecastProjectionCompositeCalculator();

        var input = new ForecastProjectionCompositeInput(
            baselineConsumption: 120m,
            currentObservedConsumption: 108m,
            previousObservedConsumption: 90m,
            threshold: 120m);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal(1.2m, output.TrendFactor);
        Assert.Equal(144m, output.ProjectedConsumption);
        Assert.Equal(24m, output.ThresholdVariance);
    }

    [Fact]
    public void ConfidenceComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new ConfidenceCompositeCalculator();

        var input = new ConfidenceCompositeInput(
            observedValues: [10m, 14m, 13m],
            spreadUpperBound: 8m,
            minConfidence: 0m,
            maxConfidence: 1m);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal(0m, output.ConfidenceScore);
    }

    [Fact]
    public void ScenarioScoreComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new ScenarioScoreCompositeCalculator();

        var input = new ScenarioScoreCompositeInput(
            componentValues: [0.8m, 0.6m],
            componentWeights: [3m, 1m],
            clampMin: 0m,
            clampMax: 1m);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal(0.75m, output.CompositeScenarioScore);
    }

    [Fact]
    public void ScenarioRankingComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new ScenarioRankingCompositeCalculator();

        var input = new ScenarioRankingCompositeInput(
            primaryScores: [0.9m, 0.9m, 0.7m],
            secondaryScores: [0.2m, 0.8m, 0.1m],
            topCount: 2);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal([1, 0], output.RankedScenarioCollection.ToArray());
    }

    [Fact]
    public void CanonicalImportConversionComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new CanonicalImportConversionCompositeCalculator();

        var input = new CanonicalImportConversionCompositeInput(
            rawDate: "2026-08-04",
            rawNumber: "42.5000",
            rawBoolean: "TRUE",
            numberRangeMin: 0m,
            numberRangeMax: 100m,
            inclusiveMin: true,
            inclusiveMax: true);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal("2026-08-04", output.CanonicalDate);
        Assert.Equal("42.5", output.CanonicalNumber);
        Assert.Equal("true", output.CanonicalBoolean);
        Assert.True(output.IsCanonicalNumberInRange);
    }

    [Fact]
    public void PaginationComposite_ShouldProduceExpectedOutputs()
    {
        var calculator = new PaginationCompositeCalculator();

        var input = new PaginationCompositeInput(
            requestedPage: 10m,
            minimumPage: 1m,
            maximumPage: 5m,
            currentItemCount: 20m,
            totalItemCount: 80m,
            pageSize: 20m);

        var output = calculator.Calculate(input, DefaultContext);

        Assert.Equal(5, output.SafePageNumber);
        Assert.False(output.IsPageValid);
        Assert.Equal(0.25m, output.PageCoverageRatio);
        Assert.Equal(4, output.TotalPageCount);
    }

    [Fact]
    public void CompositeCalculators_ShouldBeDeterministicAndStateless()
    {
        var calculator = new ScenarioScoreCompositeCalculator();
        var input = new ScenarioScoreCompositeInput([0.8m, 0.6m], [3m, 1m], 0m, 1m);

        var first = calculator.Calculate(input, DefaultContext);
        var second = calculator.Calculate(input, DefaultContext);

        Assert.Equal(first.CompositeScenarioScore, second.CompositeScenarioScore);
    }
}
