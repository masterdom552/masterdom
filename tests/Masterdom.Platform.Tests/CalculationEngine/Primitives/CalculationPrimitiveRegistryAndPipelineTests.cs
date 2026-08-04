using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Execution;
using Masterdom.Platform.CalculationEngine.Metadata;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.Tests.CalculationEngine.Primitives;

public sealed class CalculationPrimitiveRegistryAndPipelineTests
{
    private static readonly string[] ExpectedCapabilityIds =
    [
        PrimitiveCapabilityIds.AggregationSum,
        PrimitiveCapabilityIds.AggregationMean,
        PrimitiveCapabilityIds.AggregationWeightedMean,
        PrimitiveCapabilityIds.AggregationMin,
        PrimitiveCapabilityIds.AggregationMax,
        PrimitiveCapabilityIds.NormalizationClamp,
        PrimitiveCapabilityIds.NormalizationRatio,
        PrimitiveCapabilityIds.NormalizationBoundsGuard,
        PrimitiveCapabilityIds.StatisticsSpread,
        PrimitiveCapabilityIds.InterpolationWeightedBlend,
        PrimitiveCapabilityIds.InterpolationReliabilityBlend,
        PrimitiveCapabilityIds.ProjectionTrendFactor,
        PrimitiveCapabilityIds.ProjectionThresholdVariance,
        PrimitiveCapabilityIds.ValidationThreshold,
        PrimitiveCapabilityIds.ValidationRange,
        PrimitiveCapabilityIds.RankingOrder,
        PrimitiveCapabilityIds.RankingTopN,
        PrimitiveCapabilityIds.RankingTieBreak,
        PrimitiveCapabilityIds.ScoringWeightedScore,
        PrimitiveCapabilityIds.ScoringConfidence,
        PrimitiveCapabilityIds.TransformationCanonicalDate,
        PrimitiveCapabilityIds.TransformationCanonicalNumber,
        PrimitiveCapabilityIds.TransformationCanonicalBoolean
    ];

    [Fact]
    public void MetadataRegistry_ShouldContain_AllLevel1PrimitiveDescriptors()
    {
        var registry = new CalculationOperationRegistry();

        var found = ExpectedCapabilityIds
            .Select(capabilityId => registry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create(capabilityId)))
            .ToArray();

        Assert.Equal(ExpectedCapabilityIds.Length, found.Length);
        Assert.All(found, descriptor => Assert.Equal(CalculationOperationCategory.Primitive, descriptor.OperationCategory));
        Assert.All(found, descriptor => Assert.False(descriptor.CapabilityId.IsDefault));
    }

    [Fact]
    public void PrimitiveExecutionRegistry_ShouldRegister_AllLevel1Capabilities()
    {
        var executionRegistry = CalculationPrimitiveExecutionRegistryBuilder.CreateDefault();

        foreach (var capabilityId in ExpectedCapabilityIds)
        {
            var resolved = executionRegistry.TryResolve(
                CalculationOperationCapabilityId.Create(capabilityId),
                out var operation);

            Assert.True(resolved, $"Capability '{capabilityId}' was not registered.");
            Assert.NotNull(operation);
        }
    }

    [Theory]
    [MemberData(nameof(PrimitivePipelineCases))]
    public void Pipeline_ShouldExecute_AllLevel1Primitives(
        string capabilityId,
        IReadOnlyDictionary<string, object?> input,
        string expectedKey,
        object expectedValue)
    {
        var metadataRegistry = new CalculationOperationRegistry();
        var descriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create(capabilityId));

        var engine = CalculationEngineFactory.CreateDefault();
        var request = new CalculationRequest(
            descriptor.DescriptorId,
            new CalculationContext(new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc)),
            new CalculationInput(new Dictionary<string, object?>(input, StringComparer.OrdinalIgnoreCase)));

        var result = engine.Execute(request);

        Assert.Equal(descriptor.DescriptorId.Value, result.ExecutionMetadata.OperationId.Value);
        Assert.Equal(descriptor.CapabilityId.Value, result.ExecutionMetadata.CapabilityId.Value);
        Assert.Equal(descriptor.CapabilityCategory, result.ExecutionMetadata.CapabilityCategory);
        Assert.Equal(descriptor.CompatibilityStatus, result.ExecutionMetadata.CompatibilityStatus);

        Assert.True(result.Output.Values.ContainsKey(expectedKey));

        var actual = result.Output.Values[expectedKey];
        AssertPrimitiveValue(actual, expectedValue);
    }

    [Fact]
    public void DescriptorValidation_ShouldPreserveFrozenCapabilityIdentifiers()
    {
        var descriptors = new CalculationOperationRegistry()
            .GetAll()
            .Where(x => ExpectedCapabilityIds.Contains(x.CapabilityId.Value, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(ExpectedCapabilityIds.Length, descriptors.Length);
        Assert.All(descriptors, descriptor => Assert.False(string.IsNullOrWhiteSpace(descriptor.CapabilityId.Value)));
        Assert.All(descriptors, descriptor => Assert.False(string.IsNullOrWhiteSpace(descriptor.OperationVersion.Value)));
    }

    public static IEnumerable<object[]> PrimitivePipelineCases()
    {
        yield return [PrimitiveCapabilityIds.AggregationSum, Input(("values", new decimal[] { 2m, 3m, 4m })), "value", 9m];
        yield return [PrimitiveCapabilityIds.AggregationMean, Input(("values", new decimal[] { 2m, 4m, 6m })), "value", 4m];
        yield return [PrimitiveCapabilityIds.AggregationWeightedMean, Input(("values", new decimal[] { 10m, 20m }), ("weights", new decimal[] { 1m, 3m })), "value", 17.5m];
        yield return [PrimitiveCapabilityIds.AggregationMin, Input(("values", new decimal[] { 7m, -2m, 1m })), "value", -2m];
        yield return [PrimitiveCapabilityIds.AggregationMax, Input(("values", new decimal[] { 7m, -2m, 1m })), "value", 7m];

        yield return [PrimitiveCapabilityIds.NormalizationClamp, Input(("value", 12m), ("min", 0m), ("max", 10m)), "value", 10m];
        yield return [PrimitiveCapabilityIds.NormalizationRatio, Input(("numerator", 3m), ("denominator", 2m)), "value", 1.5m];
        yield return [PrimitiveCapabilityIds.NormalizationBoundsGuard, Input(("value", 12m), ("min", 0m), ("max", 10m)), "is_valid", false];

        yield return [PrimitiveCapabilityIds.StatisticsSpread, Input(("values", new decimal[] { 5m, 11m, 7m })), "value", 6m];

        yield return [PrimitiveCapabilityIds.InterpolationWeightedBlend, Input(("left", 10m), ("right", 20m), ("weight", 0.25m)), "value", 12.5m];
        yield return [PrimitiveCapabilityIds.InterpolationReliabilityBlend, Input(("values", new decimal[] { 5m, 15m }), ("reliabilities", new decimal[] { 1m, 3m })), "value", 12.5m];

        yield return [PrimitiveCapabilityIds.ProjectionTrendFactor, Input(("baseline", 100m), ("trend_factor", 1.1m)), "value", 110m];
        yield return [PrimitiveCapabilityIds.ProjectionThresholdVariance, Input(("projected", 110m), ("threshold", 95m)), "value", 15m];

        yield return [PrimitiveCapabilityIds.ValidationThreshold, Input(("value", 5m), ("threshold", 4m), ("operator", "gt")), "is_valid", true];
        yield return [PrimitiveCapabilityIds.ValidationRange, Input(("value", 5m), ("min", 5m), ("max", 10m), ("inclusive_min", true), ("inclusive_max", false)), "is_valid", true];

        yield return [PrimitiveCapabilityIds.RankingOrder, Input(("values", new decimal[] { 1m, 3m, 2m }), ("descending", true)), "ordered_values", new decimal[] { 3m, 2m, 1m }];
        yield return [PrimitiveCapabilityIds.RankingTopN, Input(("ordered_values", new decimal[] { 9m, 7m, 4m }), ("count", 2)), "selected_values", new decimal[] { 9m, 7m }];
        yield return [PrimitiveCapabilityIds.RankingTieBreak, Input(("primary_scores", new decimal[] { 10m, 10m, 8m }), ("secondary_scores", new decimal[] { 3m, 1m, 9m })), "ordered_indices", new[] { 0, 1, 2 }];

        yield return [PrimitiveCapabilityIds.ScoringWeightedScore, Input(("values", new decimal[] { 0.8m, 0.6m }), ("weights", new decimal[] { 3m, 1m })), "value", 0.75m];
        yield return [PrimitiveCapabilityIds.ScoringConfidence, Input(("quality", 0.9m), ("penalty", 0.15m), ("min", 0m), ("max", 1m)), "value", 0.75m];

        yield return [PrimitiveCapabilityIds.TransformationCanonicalDate, Input(("value", "2026-08-04")), "canonical_date", "2026-08-04"];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalNumber, Input(("value", "42.5000")), "canonical_number", "42.5"];
        yield return [PrimitiveCapabilityIds.TransformationCanonicalBoolean, Input(("value", "TRUE")), "canonical_boolean", "true"];
    }

    private static Dictionary<string, object?> Input(params (string key, object? value)[] pairs)
    {
        return pairs.ToDictionary(pair => pair.key, pair => pair.value, StringComparer.OrdinalIgnoreCase);
    }

    private static void AssertPrimitiveValue(object? actual, object expected)
    {
        if (expected is decimal expectedDecimal)
        {
            Assert.IsType<decimal>(actual);
            Assert.Equal(expectedDecimal, (decimal)actual!);
            return;
        }

        if (expected is bool expectedBool)
        {
            Assert.IsType<bool>(actual);
            Assert.Equal(expectedBool, (bool)actual!);
            return;
        }

        if (expected is decimal[] expectedDecimalArray)
        {
            var actualDecimalArray = Assert.IsAssignableFrom<IEnumerable<decimal>>(actual).ToArray();
            Assert.Equal(expectedDecimalArray, actualDecimalArray);
            return;
        }

        if (expected is int[] expectedIntArray)
        {
            var actualIntArray = Assert.IsAssignableFrom<IEnumerable<int>>(actual).ToArray();
            Assert.Equal(expectedIntArray, actualIntArray);
            return;
        }

        Assert.Equal(expected, actual);
    }
}
