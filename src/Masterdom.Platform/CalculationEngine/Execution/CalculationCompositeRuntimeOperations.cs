using CalcComposites = Masterdom.Platform.CalculationEngine.
    Composites;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal sealed class ConsumptionEstimationCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.IConsumptionEstimationCompositeCalculator _calculator;

    internal ConsumptionEstimationCompositeOperation(CalcComposites.IConsumptionEstimationCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.ConsumptionEstimationCompositeInput(
            PrimitiveInput.GetDecimalArray(request.Input.Values, "historicalValues"),
            PrimitiveInput.GetDecimalArray(request.Input.Values, "historicalWeights"),
            PrimitiveInput.GetDecimal(request.Input.Values, "blendWeight"),
            PrimitiveInput.GetDecimal(request.Input.Values, "occupancyNumerator"),
            PrimitiveInput.GetDecimal(request.Input.Values, "occupancyDenominator"),
            PrimitiveInput.GetDecimal(request.Input.Values, "completenessObservedCount"),
            PrimitiveInput.GetDecimal(request.Input.Values, "completenessExpectedCount"),
            PrimitiveInput.GetDecimal(request.Input.Values, "clampMin"),
            PrimitiveInput.GetDecimal(request.Input.Values, "clampMax"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.ConsumptionEstimation,
            CalculationOperationCapabilityCategory.Aggregation,
            CalculationOperationCompatibilityStatus.Supported,
            new Dictionary<string, object?>
            {
                ["estimatedBaseline"] = output.EstimatedBaseline,
                ["failedMeterEstimate"] = output.FailedMeterEstimate,
                ["occupancyAdjustedEstimate"] = output.OccupancyAdjustedEstimate,
                ["dataCompletenessRatio"] = output.DataCompletenessRatio
            });
    }
}

internal sealed class ForecastProjectionCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.IForecastProjectionCompositeCalculator _calculator;

    internal ForecastProjectionCompositeOperation(CalcComposites.IForecastProjectionCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.ForecastProjectionCompositeInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "baselineConsumption"),
            PrimitiveInput.GetDecimal(request.Input.Values, "currentObservedConsumption"),
            PrimitiveInput.GetDecimal(request.Input.Values, "previousObservedConsumption"),
            PrimitiveInput.GetDecimal(request.Input.Values, "threshold"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.ForecastProjection,
            CalculationOperationCapabilityCategory.Projection,
            CalculationOperationCompatibilityStatus.Supported,
            new Dictionary<string, object?>
            {
                ["trendFactor"] = output.TrendFactor,
                ["projectedConsumption"] = output.ProjectedConsumption,
                ["thresholdVariance"] = output.ThresholdVariance
            });
    }
}

internal sealed class ConfidenceCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.IConfidenceCompositeCalculator _calculator;

    internal ConfidenceCompositeOperation(CalcComposites.IConfidenceCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.ConfidenceCompositeInput(
            PrimitiveInput.GetDecimalArray(request.Input.Values, "observedValues"),
            PrimitiveInput.GetDecimal(request.Input.Values, "spreadUpperBound"),
            PrimitiveInput.GetDecimal(request.Input.Values, "minConfidence"),
            PrimitiveInput.GetDecimal(request.Input.Values, "maxConfidence"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.Confidence,
            CalculationOperationCapabilityCategory.Scoring,
            CalculationOperationCompatibilityStatus.Supported,
            new Dictionary<string, object?>
            {
                ["confidenceScore"] = output.ConfidenceScore
            });
    }
}

internal sealed class ScenarioScoreCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.IScenarioScoreCompositeCalculator _calculator;

    internal ScenarioScoreCompositeOperation(CalcComposites.IScenarioScoreCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.ScenarioScoreCompositeInput(
            PrimitiveInput.GetDecimalArray(request.Input.Values, "componentValues"),
            PrimitiveInput.GetDecimalArray(request.Input.Values, "componentWeights"),
            PrimitiveInput.GetDecimal(request.Input.Values, "clampMin"),
            PrimitiveInput.GetDecimal(request.Input.Values, "clampMax"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.ScenarioScore,
            CalculationOperationCapabilityCategory.Scoring,
            CalculationOperationCompatibilityStatus.Supported,
            new Dictionary<string, object?>
            {
                ["compositeScenarioScore"] = output.CompositeScenarioScore
            });
    }
}

internal sealed class ScenarioRankingCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.IScenarioRankingCompositeCalculator _calculator;

    internal ScenarioRankingCompositeOperation(CalcComposites.IScenarioRankingCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.ScenarioRankingCompositeInput(
            PrimitiveInput.GetDecimalArray(request.Input.Values, "primaryScores"),
            PrimitiveInput.GetDecimalArray(request.Input.Values, "secondaryScores"),
            PrimitiveInput.GetInt(request.Input.Values, "topCount"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.ScenarioRanking,
            CalculationOperationCapabilityCategory.Ranking,
            CalculationOperationCompatibilityStatus.Supported,
            new Dictionary<string, object?>
            {
                ["rankedScenarioCollection"] = output.RankedScenarioCollection
            });
    }
}

internal sealed class CanonicalImportConversionCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.ICanonicalImportConversionCompositeCalculator _calculator;

    internal CanonicalImportConversionCompositeOperation(CalcComposites.ICanonicalImportConversionCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.CanonicalImportConversionCompositeInput(
            PrimitiveInput.GetAny(request.Input.Values, "rawDate"),
            PrimitiveInput.GetAny(request.Input.Values, "rawNumber"),
            PrimitiveInput.GetAny(request.Input.Values, "rawBoolean"),
            PrimitiveInput.GetDecimal(request.Input.Values, "numberRangeMin"),
            PrimitiveInput.GetDecimal(request.Input.Values, "numberRangeMax"),
            PrimitiveInput.GetBool(request.Input.Values, "inclusiveMin"),
            PrimitiveInput.GetBool(request.Input.Values, "inclusiveMax"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.CanonicalImportConversion,
            CalculationOperationCapabilityCategory.Transformation,
            CalculationOperationCompatibilityStatus.Supported,
            new Dictionary<string, object?>
            {
                ["canonicalDate"] = output.CanonicalDate,
                ["canonicalNumber"] = output.CanonicalNumber,
                ["canonicalBoolean"] = output.CanonicalBoolean,
                ["isCanonicalNumberInRange"] = output.IsCanonicalNumberInRange
            });
    }
}

internal sealed class PaginationCompositeOperation : ICalculationComposite
{
    private readonly CalcComposites.IPaginationCompositeCalculator _calculator;

    internal PaginationCompositeOperation(CalcComposites.IPaginationCompositeCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var input = new CalcComposites.PaginationCompositeInput(
            PrimitiveInput.GetDecimal(request.Input.Values, "requestedPage"),
            PrimitiveInput.GetDecimal(request.Input.Values, "minimumPage"),
            PrimitiveInput.GetDecimal(request.Input.Values, "maximumPage"),
            PrimitiveInput.GetDecimal(request.Input.Values, "currentItemCount"),
            PrimitiveInput.GetDecimal(request.Input.Values, "totalItemCount"),
            PrimitiveInput.GetDecimal(request.Input.Values, "pageSize"));

        var output = _calculator.Calculate(input, request.Context);

        return CompositeRuntimeResult.Create(
            request,
            CalcComposites.CompositeCapabilityIds.Pagination,
            CalculationOperationCapabilityCategory.Validation,
            CalculationOperationCompatibilityStatus.Obsolete,
            new Dictionary<string, object?>
            {
                ["safePageNumber"] = output.SafePageNumber,
                ["isPageValid"] = output.IsPageValid,
                ["pageCoverageRatio"] = output.PageCoverageRatio,
                ["totalPageCount"] = output.TotalPageCount
            });
    }
}

internal static class CompositeRuntimeResult
{
    internal static CalculationResult Create(
        ICalculationRequest request,
        string capabilityId,
        CalculationOperationCapabilityCategory capabilityCategory,
        CalculationOperationCompatibilityStatus compatibilityStatus,
        IReadOnlyDictionary<string, object?> outputValues)
    {
        return new CalculationResult(
            new CalculationOutput(outputValues),
            new CalculationExecutionMetadata(
                request.OperationId,
                CalculationOperationVersion.Create("1.0.0"),
                DateTime.UtcNow,
                TimeSpan.Zero,
                CalculationOperationCapabilityId.Create(capabilityId),
                capabilityCategory,
                compatibilityStatus));
    }
}
