using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;
using Masterdom.Platform.CalculationEngine.Primitives;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal static class CalculationPrimitiveExecutionRegistryBuilder
{
    private static readonly IReadOnlyDictionary<string, ICalculationPrimitive> PrimitiveOperations =
        new Dictionary<string, ICalculationPrimitive>(StringComparer.OrdinalIgnoreCase)
        {
            [PrimitiveCapabilityIds.AggregationSum] = new AggregationSumPrimitive(),
            [PrimitiveCapabilityIds.AggregationMean] = new AggregationMeanPrimitive(),
            [PrimitiveCapabilityIds.AggregationWeightedMean] = new AggregationWeightedMeanPrimitive(),
            [PrimitiveCapabilityIds.AggregationMin] = new AggregationMinimumPrimitive(),
            [PrimitiveCapabilityIds.AggregationMax] = new AggregationMaximumPrimitive(),
            [PrimitiveCapabilityIds.NormalizationClamp] = new NormalizationClampPrimitive(),
            [PrimitiveCapabilityIds.NormalizationRatio] = new NormalizationRatioPrimitive(),
            [PrimitiveCapabilityIds.NormalizationBoundsGuard] = new NormalizationBoundsGuardPrimitive(),
            [PrimitiveCapabilityIds.StatisticsSpread] = new StatisticsSpreadPrimitive(),
            [PrimitiveCapabilityIds.InterpolationWeightedBlend] = new InterpolationWeightedBlendPrimitive(),
            [PrimitiveCapabilityIds.InterpolationReliabilityBlend] = new InterpolationReliabilityBlendPrimitive(),
            [PrimitiveCapabilityIds.ProjectionTrendFactor] = new ProjectionTrendFactorPrimitive(),
            [PrimitiveCapabilityIds.ProjectionThresholdVariance] = new ProjectionThresholdVariancePrimitive(),
            [PrimitiveCapabilityIds.ValidationThreshold] = new ValidationThresholdPrimitive(),
            [PrimitiveCapabilityIds.ValidationRange] = new ValidationRangePrimitive(),
            [PrimitiveCapabilityIds.RankingOrder] = new RankingOrderPrimitive(),
            [PrimitiveCapabilityIds.RankingTopN] = new RankingTopNPrimitive(),
            [PrimitiveCapabilityIds.RankingTieBreak] = new RankingTieBreakPrimitive(),
            [PrimitiveCapabilityIds.ScoringWeightedScore] = new ScoringWeightedScorePrimitive(),
            [PrimitiveCapabilityIds.ScoringConfidence] = new ScoringConfidencePrimitive(),
            [PrimitiveCapabilityIds.TransformationCanonicalDate] = new TransformationCanonicalDatePrimitive(),
            [PrimitiveCapabilityIds.TransformationCanonicalNumber] = new TransformationCanonicalNumberPrimitive(),
            [PrimitiveCapabilityIds.TransformationCanonicalBoolean] = new TransformationCanonicalBooleanPrimitive()
        };

    internal static ICalculationExecutionRegistry CreateDefault()
    {
        var metadataRegistry = new CalculationOperationRegistry();

        var primitiveDescriptors = metadataRegistry
            .GetAll()
            .Where(descriptor => descriptor.OperationCategory is CalculationOperationCategory.Primitive)
            .Where(descriptor => PrimitiveOperations.ContainsKey(descriptor.CapabilityId.Value))
            .ToArray();

        var missingDescriptors = PrimitiveOperations.Keys
            .Where(capabilityId => primitiveDescriptors.All(descriptor =>
                !string.Equals(descriptor.CapabilityId.Value, capabilityId, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (missingDescriptors.Length > 0)
        {
            throw new CalculationOperationValidationException(
                $"Level 1 primitive descriptors are missing for capability ids: {string.Join(", ", missingDescriptors)}.");
        }

        var definitions = primitiveDescriptors
            .Select(descriptor =>
                new CalculationOperationExecutionDefinition(
                    PrimitiveOperations[descriptor.CapabilityId.Value],
                    descriptor))
            .ToArray();

        return new CalculationExecutionRegistry(definitions);
    }

    internal static IReadOnlyDictionary<string, ICalculationPrimitive> GetRegisteredOperations()
    {
        return PrimitiveOperations;
    }
}
