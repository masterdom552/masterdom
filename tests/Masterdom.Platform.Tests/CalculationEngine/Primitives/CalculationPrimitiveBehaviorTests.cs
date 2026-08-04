using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Execution;
using Masterdom.Platform.CalculationEngine.Metadata;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.Tests.CalculationEngine.Primitives;

public sealed class CalculationPrimitiveBehaviorTests
{
    [Theory]
    [MemberData(nameof(DeterministicCases))]
    public void PrimitiveOperations_ShouldBeDeterministic_AndPure(
        string capabilityId,
        IReadOnlyDictionary<string, object?> input,
        string outputKey)
    {
        var operation = CalculationPrimitiveExecutionRegistryBuilder.GetRegisteredOperations()[capabilityId];

        var cloned = new Dictionary<string, object?>(input, StringComparer.OrdinalIgnoreCase);

        var request = new CalculationRequest(
            CalculationOperationDescriptorId.Create("ce-op-test"),
            new CalculationContext(new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc)),
            new CalculationInput(cloned));

        var first = operation.Execute(request);
        var second = operation.Execute(request);

        Assert.True(first.Output.Values.ContainsKey(outputKey));
        Assert.True(second.Output.Values.ContainsKey(outputKey));
        Assert.Equal(first.Output.Values[outputKey]?.ToString(), second.Output.Values[outputKey]?.ToString());

        Assert.Equal(input.Count, cloned.Count);
        foreach (var pair in input)
        {
            Assert.True(cloned.ContainsKey(pair.Key));
            Assert.Equal(pair.Value?.ToString(), cloned[pair.Key]?.ToString());
        }
    }

    [Theory]
    [MemberData(nameof(InvalidCases))]
    public void PrimitiveOperations_ShouldRejectInvalidInputs(
        string capabilityId,
        IReadOnlyDictionary<string, object?> input)
    {
        var operation = CalculationPrimitiveExecutionRegistryBuilder.GetRegisteredOperations()[capabilityId];

        var request = new CalculationRequest(
            CalculationOperationDescriptorId.Create("ce-op-test"),
            new CalculationContext(new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc)),
            new CalculationInput(new Dictionary<string, object?>(input, StringComparer.OrdinalIgnoreCase)));

        Assert.ThrowsAny<Exception>(() => operation.Execute(request));
    }

    [Fact]
    public void AggregationSum_ShouldThrowOnOverflow()
    {
        var operation = new AggregationSumPrimitive();

        var request = new CalculationRequest(
            CalculationOperationDescriptorId.Create("ce-op-test"),
            new CalculationContext(new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc)),
            new CalculationInput(new Dictionary<string, object?>
            {
                ["values"] = new[] { decimal.MaxValue, 1m }
            }));

        Assert.Throws<OverflowException>(() => operation.Execute(request));
    }

    [Fact]
    public void NormalizationClamp_ShouldHonorBoundaryValues()
    {
        var operation = new NormalizationClampPrimitive();

        var atMin = Execute(operation, ("value", 0m), ("min", 0m), ("max", 10m));
        var atMax = Execute(operation, ("value", 10m), ("min", 0m), ("max", 10m));

        Assert.Equal(0m, atMin["value"]);
        Assert.Equal(10m, atMax["value"]);
    }

    [Fact]
    public void RankingTopN_ShouldReturnAll_WhenCountExceedsLength()
    {
        var operation = new RankingTopNPrimitive();
        var output = Execute(operation, ("ordered_values", new decimal[] { 5m, 4m }), ("count", 10));

        Assert.Equal(new decimal[] { 5m, 4m }, Assert.IsAssignableFrom<IEnumerable<decimal>>(output["selected_values"]).ToArray());
    }

    [Fact]
    public void ValidationRange_ShouldSupportExclusiveBounds()
    {
        var operation = new ValidationRangePrimitive();

        var output = Execute(
            operation,
            ("value", 5m),
            ("min", 5m),
            ("max", 6m),
            ("inclusive_min", false),
            ("inclusive_max", true));

        Assert.False((bool)output["is_valid"]!);
    }

    [Fact]
    public void TransformationCanonicalBoolean_ShouldAcceptNumericTokens()
    {
        var operation = new TransformationCanonicalBooleanPrimitive();

        var trueOutput = Execute(operation, ("value", 1));
        var falseOutput = Execute(operation, ("value", "0"));

        Assert.Equal("true", trueOutput["canonical_boolean"]);
        Assert.Equal("false", falseOutput["canonical_boolean"]);
    }

    public static IEnumerable<object[]> DeterministicCases()
    {
        yield return [PrimitiveCapabilityIds.AggregationSum, Input(("values", new decimal[] { 1m, 2m, 3m })), "value"];
        yield return [PrimitiveCapabilityIds.AggregationMean, Input(("values", new decimal[] { 2m, 4m })), "value"];
        yield return [PrimitiveCapabilityIds.AggregationWeightedMean, Input(("values", new decimal[] { 10m, 20m }), ("weights", new decimal[] { 1m, 3m })), "value"];
        yield return [PrimitiveCapabilityIds.AggregationMin, Input(("values", new decimal[] { -1m, 4m })), "value"];
        yield return [PrimitiveCapabilityIds.AggregationMax, Input(("values", new decimal[] { -1m, 4m })), "value"];
        yield return [PrimitiveCapabilityIds.NormalizationClamp, Input(("value", 12m), ("min", 0m), ("max", 10m)), "value"];
        yield return [PrimitiveCapabilityIds.NormalizationRatio, Input(("numerator", 3m), ("denominator", 2m)), "value"];
        yield return [PrimitiveCapabilityIds.NormalizationBoundsGuard, Input(("value", 12m), ("min", 0m), ("max", 10m)), "is_valid"];
        yield return [PrimitiveCapabilityIds.StatisticsSpread, Input(("values", new decimal[] { 1m, 3m })), "value"];
        yield return [PrimitiveCapabilityIds.InterpolationWeightedBlend, Input(("left", 1m), ("right", 3m), ("weight", 0.5m)), "value"];
        yield return [PrimitiveCapabilityIds.InterpolationReliabilityBlend, Input(("values", new decimal[] { 1m, 3m }), ("reliabilities", new decimal[] { 1m, 1m })), "value"];
        yield return [PrimitiveCapabilityIds.ProjectionTrendFactor, Input(("baseline", 10m), ("trend_factor", 2m)), "value"];
        yield return [PrimitiveCapabilityIds.ProjectionThresholdVariance, Input(("projected", 10m), ("threshold", 7m)), "value"];
        yield return [PrimitiveCapabilityIds.ValidationThreshold, Input(("value", 5m), ("threshold", 4m), ("operator", "gt")), "is_valid"];
        yield return [PrimitiveCapabilityIds.ValidationRange, Input(("value", 5m), ("min", 4m), ("max", 6m)), "is_valid"];
        yield return [PrimitiveCapabilityIds.RankingOrder, Input(("values", new decimal[] { 3m, 1m }), ("descending", true)), "ordered_values"];
        yield return [PrimitiveCapabilityIds.RankingTopN, Input(("ordered_values", new decimal[] { 3m, 1m }), ("count", 1)), "selected_values"];
        yield return [PrimitiveCapabilityIds.RankingTieBreak, Input(("primary_scores", new decimal[] { 2m, 2m }), ("secondary_scores", new decimal[] { 1m, 0m })), "ordered_indices"];
        yield return [PrimitiveCapabilityIds.ScoringWeightedScore, Input(("values", new decimal[] { 0.4m, 0.8m }), ("weights", new decimal[] { 1m, 2m })), "value"];
        yield return [PrimitiveCapabilityIds.ScoringConfidence, Input(("quality", 0.9m), ("penalty", 0.1m)), "value"];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalDate, Input(("value", "2026-08-04")), "canonical_date"];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalNumber, Input(("value", "42.0")), "canonical_number"];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalBoolean, Input(("value", "true")), "canonical_boolean"];
    }

    public static IEnumerable<object[]> InvalidCases()
    {
        yield return [PrimitiveCapabilityIds.AggregationSum, Input(("values", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.AggregationMean, Input(("values", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.AggregationWeightedMean, Input(("values", new[] { 1m }), ("weights", new[] { 1m, 2m }))];
        yield return [PrimitiveCapabilityIds.AggregationMin, Input(("values", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.AggregationMax, Input(("values", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.NormalizationClamp, Input(("value", 1m), ("min", 2m), ("max", 1m))];
        yield return [PrimitiveCapabilityIds.NormalizationRatio, Input(("numerator", 1m), ("denominator", 0m))];
        yield return [PrimitiveCapabilityIds.NormalizationBoundsGuard, Input(("value", 1m), ("min", 2m), ("max", 1m))];
        yield return [PrimitiveCapabilityIds.StatisticsSpread, Input(("values", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.InterpolationWeightedBlend, Input(("left", 1m), ("right", 2m), ("weight", 1.5m))];
        yield return [PrimitiveCapabilityIds.InterpolationReliabilityBlend, Input(("values", new[] { 1m, 2m }), ("reliabilities", new[] { 0m, 0m }))];
        yield return [PrimitiveCapabilityIds.ProjectionTrendFactor, Input(("baseline", "x"), ("trend_factor", 2m))];
        yield return [PrimitiveCapabilityIds.ProjectionThresholdVariance, Input(("projected", 1m))];
        yield return [PrimitiveCapabilityIds.ValidationThreshold, Input(("value", 1m), ("threshold", 2m), ("operator", "bad"))];
        yield return [PrimitiveCapabilityIds.ValidationRange, Input(("value", 1m), ("min", 2m), ("max", 1m))];
        yield return [PrimitiveCapabilityIds.RankingOrder, Input(("values", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.RankingTopN, Input(("ordered_values", new[] { 1m }), ("count", -1))];
        yield return [PrimitiveCapabilityIds.RankingTieBreak, Input(("primary_scores", new[] { 1m }), ("secondary_scores", Array.Empty<decimal>()))];
        yield return [PrimitiveCapabilityIds.ScoringWeightedScore, Input(("values", new[] { 1m }), ("weights", new[] { 0m }))];
        yield return [PrimitiveCapabilityIds.ScoringConfidence, Input(("quality", 0.7m), ("penalty", 0.2m), ("min", 1m), ("max", 0m))];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalDate, Input(("value", "not-a-date"))];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalNumber, Input(("value", "nan"))];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalBoolean, Input(("value", "maybe"))];
    }

    private static Dictionary<string, object?> Execute(ICalculationOperation operation, params (string key, object? value)[] values)
    {
        var request = new CalculationRequest(
            CalculationOperationDescriptorId.Create("ce-op-test"),
            new CalculationContext(new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc)),
            new CalculationInput(Input(values)));

        return operation.Execute(request).Output.Values.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private static Dictionary<string, object?> Input(params (string key, object? value)[] values)
    {
        return values.ToDictionary(pair => pair.key, pair => pair.value, StringComparer.OrdinalIgnoreCase);
    }
}
