using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

internal static class CalculationOperationDescriptorSourceFactory
{
    internal static CalculationOperationDescriptor CreatePrimitive(
        string descriptorId,
        string operationName,
        string capabilityId,
        string operationVersion,
        string description,
        CalculationOperationPrimitiveFamily primitiveFamily,
        CalculationOperationCapabilityCategory capabilityCategory,
        CalculationOperationStability stability,
        string timeComplexity,
        string spaceComplexity,
        IReadOnlyList<string> dependencyCapabilityIds,
        IReadOnlyList<string> technicalTags,
        IReadOnlyList<string> mathematicalTags,
        CalculationOperationCompatibilityStatus compatibilityStatus = CalculationOperationCompatibilityStatus.Supported)
    {
        return CreateDescriptor(
            descriptorId,
            operationName,
            capabilityId,
            operationVersion,
            description,
            primitiveFamily,
            capabilityCategory,
            CalculationOperationCompositionLevel.Primitive,
            CalculationOperationCategory.Primitive,
            CalculationOperationExecutionClassification.Primitive,
            CalculationOperationPurity.Pure,
            CalculationOperationDeterminism.Deterministic,
            stability,
            timeComplexity,
            spaceComplexity,
            dependencyCapabilityIds,
            technicalTags,
            mathematicalTags,
            compatibilityStatus);
    }

    internal static CalculationOperationDescriptor CreateComposite(
        string descriptorId,
        string operationName,
        string capabilityId,
        string operationVersion,
        string description,
        CalculationOperationPrimitiveFamily primitiveFamily,
        CalculationOperationCapabilityCategory capabilityCategory,
        CalculationOperationStability stability,
        string timeComplexity,
        string spaceComplexity,
        IReadOnlyList<string> dependencyCapabilityIds,
        IReadOnlyList<string> technicalTags,
        IReadOnlyList<string> mathematicalTags,
        CalculationOperationCompatibilityStatus compatibilityStatus = CalculationOperationCompatibilityStatus.Supported)
    {
        return CreateDescriptor(
            descriptorId,
            operationName,
            capabilityId,
            operationVersion,
            description,
            primitiveFamily,
            capabilityCategory,
            CalculationOperationCompositionLevel.Composite,
            CalculationOperationCategory.Composite,
            CalculationOperationExecutionClassification.Composite,
            CalculationOperationPurity.Contextual,
            CalculationOperationDeterminism.Deterministic,
            stability,
            timeComplexity,
            spaceComplexity,
            dependencyCapabilityIds,
            technicalTags,
            mathematicalTags,
            compatibilityStatus);
    }

    private static CalculationOperationDescriptor CreateDescriptor(
        string descriptorId,
        string operationName,
        string capabilityId,
        string operationVersion,
        string description,
        CalculationOperationPrimitiveFamily primitiveFamily,
        CalculationOperationCapabilityCategory capabilityCategory,
        CalculationOperationCompositionLevel compositionLevel,
        CalculationOperationCategory operationCategory,
        CalculationOperationExecutionClassification executionClassification,
        CalculationOperationPurity purity,
        CalculationOperationDeterminism determinism,
        CalculationOperationStability stability,
        string timeComplexity,
        string spaceComplexity,
        IReadOnlyList<string> dependencyCapabilityIds,
        IReadOnlyList<string> technicalTags,
        IReadOnlyList<string> mathematicalTags,
        CalculationOperationCompatibilityStatus compatibilityStatus)
    {
        return new CalculationOperationDescriptor
        {
            DescriptorId = CalculationOperationDescriptorId.Create(descriptorId),
            SourceType = CalculationOperationDescriptorSourceType.Reflection,
            SchemaVersion = "1.0",
            OperationName = operationName,
            CapabilityId = CalculationOperationCapabilityId.Create(capabilityId),
            OperationVersion = CalculationOperationVersion.Create(operationVersion),
            Description = description,
            PrimitiveFamily = primitiveFamily,
            CapabilityCategory = capabilityCategory,
            CompositionLevel = compositionLevel,
            OperationCategory = operationCategory,
            ExecutionClassification = executionClassification,
            Purity = purity,
            Determinism = determinism,
            Stability = stability,
            CompatibilityStatus = compatibilityStatus,
            TimeComplexity = timeComplexity,
            SpaceComplexity = spaceComplexity,
            DependencyCapabilityIds = dependencyCapabilityIds.Select(CalculationOperationCapabilityId.Create).ToImmutableArray(),
            TechnicalTags = technicalTags.ToImmutableArray(),
            MathematicalTags = mathematicalTags.ToImmutableArray()
        };
    }
}

internal sealed class AggregationCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00001", "Aggregation Mean", "aggregation.mean", "1.0.0", "Computes arithmetic mean of the supplied values.", CalculationOperationPrimitiveFamily.Aggregation, CalculationOperationCapabilityCategory.Aggregation, CalculationOperationStability.Fundamental, "O(n)", "O(1)", [], ["aggregation", "primitive"], ["mean", "average"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00002", "Aggregation Weighted Mean", "aggregation.weighted_mean", "1.0.0", "Computes weighted arithmetic mean of the supplied values.", CalculationOperationPrimitiveFamily.Aggregation, CalculationOperationCapabilityCategory.Aggregation, CalculationOperationStability.Stable, "O(n)", "O(1)", ["aggregation.mean"], ["aggregation", "primitive"], ["mean", "weighted", "average"], CalculationOperationCompatibilityStatus.Deprecated),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00003", "Aggregation Sum", "aggregation.sum", "1.0.0", "Computes additive total of the supplied values.", CalculationOperationPrimitiveFamily.Aggregation, CalculationOperationCapabilityCategory.Aggregation, CalculationOperationStability.Fundamental, "O(n)", "O(1)", [], ["aggregation", "primitive"], ["sum", "total"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00004", "Aggregation Minimum", "aggregation.min", "1.0.0", "Selects the smallest supplied value.", CalculationOperationPrimitiveFamily.Aggregation, CalculationOperationCapabilityCategory.Aggregation, CalculationOperationStability.Fundamental, "O(n)", "O(1)", [], ["aggregation", "primitive"], ["minimum", "min"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00005", "Aggregation Maximum", "aggregation.max", "1.0.0", "Selects the largest supplied value.", CalculationOperationPrimitiveFamily.Aggregation, CalculationOperationCapabilityCategory.Aggregation, CalculationOperationStability.Fundamental, "O(n)", "O(1)", [], ["aggregation", "primitive"], ["maximum", "max"])
        ];
    }
}

internal sealed class NormalizationCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00006", "Normalization Clamp", "normalization.clamp", "1.0.0", "Constrains a value to an explicit interval.", CalculationOperationPrimitiveFamily.Normalization, CalculationOperationCapabilityCategory.Normalization, CalculationOperationStability.Fundamental, "O(1)", "O(1)", [], ["normalization", "primitive"], ["clamp", "bounds"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00007", "Normalization Ratio", "normalization.ratio", "1.0.0", "Computes the ratio of numerator to denominator.", CalculationOperationPrimitiveFamily.Normalization, CalculationOperationCapabilityCategory.Normalization, CalculationOperationStability.Fundamental, "O(1)", "O(1)", [], ["normalization", "primitive"], ["ratio"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00008", "Normalization Bounds Guard", "normalization.bounds_guard", "1.0.0", "Validates or enforces explicit bounds.", CalculationOperationPrimitiveFamily.Normalization, CalculationOperationCapabilityCategory.Normalization, CalculationOperationStability.Fundamental, "O(1)", "O(1)", [], ["normalization", "primitive"], ["bounds", "guard"])
        ];
    }
}

internal sealed class InterpolationCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00009", "Interpolation Weighted Blend", "interpolation.weighted_blend", "1.0.0", "Interpolates between two values using an explicit blend weight.", CalculationOperationPrimitiveFamily.Interpolation, CalculationOperationCapabilityCategory.Interpolation, CalculationOperationStability.Stable, "O(1)", "O(1)", ["aggregation.mean"], ["interpolation", "primitive"], ["weighted", "blend", "linear"], CalculationOperationCompatibilityStatus.Experimental),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00010", "Interpolation Reliability Blend", "interpolation.reliability_blend", "1.0.0", "Blends values using explicit reliability weights.", CalculationOperationPrimitiveFamily.Interpolation, CalculationOperationCapabilityCategory.Interpolation, CalculationOperationStability.Stable, "O(1)", "O(1)", ["aggregation.sum"], ["interpolation", "primitive"], ["reliability", "blend"])
        ];
    }
}

internal sealed class ProjectionStatisticsCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00011", "Projection Trend Factor", "projection.trend_factor", "1.0.0", "Projects a baseline value using a multiplicative trend factor.", CalculationOperationPrimitiveFamily.Projection, CalculationOperationCapabilityCategory.Projection, CalculationOperationStability.Stable, "O(1)", "O(1)", ["normalization.ratio"], ["projection", "primitive"], ["trend", "factor"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00012", "Projection Threshold Variance", "projection.threshold_variance", "1.0.0", "Computes the difference between a projected value and a reference threshold.", CalculationOperationPrimitiveFamily.Projection, CalculationOperationCapabilityCategory.Projection, CalculationOperationStability.Stable, "O(1)", "O(1)", ["projection.trend_factor"], ["projection", "primitive"], ["threshold", "variance"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00013", "Statistics Spread", "statistics.spread", "1.0.0", "Computes the range width of a finite sequence.", CalculationOperationPrimitiveFamily.Statistics, CalculationOperationCapabilityCategory.Statistics, CalculationOperationStability.Fundamental, "O(n)", "O(1)", [], ["statistics", "primitive"], ["spread", "range"])
        ];
    }
}

internal sealed class ScoringCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00014", "Scoring Weighted Score", "scoring.weighted_score", "1.0.0", "Computes a weighted composite score from explicit components.", CalculationOperationPrimitiveFamily.Scoring, CalculationOperationCapabilityCategory.Scoring, CalculationOperationStability.Stable, "O(n)", "O(1)", ["aggregation.sum", "normalization.ratio"], ["scoring", "primitive"], ["weighted", "score"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00015", "Scoring Confidence", "scoring.confidence", "1.0.0", "Computes a bounded confidence value from quality and penalty inputs.", CalculationOperationPrimitiveFamily.Scoring, CalculationOperationCapabilityCategory.Scoring, CalculationOperationStability.Stable, "O(n)", "O(1)", ["projection.threshold_variance", "normalization.clamp"], ["scoring", "primitive"], ["confidence", "score"])
        ];
    }
}

internal sealed class RankingCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00016", "Ranking Order", "ranking.order", "1.0.0", "Produces a stable ordering using explicit key chains.", CalculationOperationPrimitiveFamily.Ranking, CalculationOperationCapabilityCategory.Ranking, CalculationOperationStability.Fundamental, "O(n log n)", "O(1)", ["aggregation.sum", "normalization.ratio"], ["ranking", "primitive"], ["order", "stable"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00017", "Ranking Tie Break", "ranking.tie_break", "1.0.0", "Resolves equal primary ranks using explicit secondary criteria.", CalculationOperationPrimitiveFamily.Ranking, CalculationOperationCapabilityCategory.Ranking, CalculationOperationStability.Stable, "O(1)", "O(1)", ["ranking.order"], ["ranking", "primitive"], ["tie", "break"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00018", "Ranking Top N", "ranking.top_n", "1.0.0", "Selects the first N items from an ordered sequence.", CalculationOperationPrimitiveFamily.Ranking, CalculationOperationCapabilityCategory.Ranking, CalculationOperationStability.Fundamental, "O(n)", "O(1)", ["ranking.order"], ["ranking", "primitive"], ["top", "selection"])
        ];
    }
}

internal sealed class TransformationCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00019", "Transformation Canonical Date", "transformation.canonical_date", "1.0.0", "Converts date text into a canonical date representation.", CalculationOperationPrimitiveFamily.Transformation, CalculationOperationCapabilityCategory.Transformation, CalculationOperationStability.Experimental, "O(1)", "O(1)", [], ["transformation", "primitive"], ["canonical", "date"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00020", "Transformation Canonical Number", "transformation.canonical_number", "1.0.0", "Converts numeric text into a canonical numeric representation.", CalculationOperationPrimitiveFamily.Transformation, CalculationOperationCapabilityCategory.Transformation, CalculationOperationStability.Experimental, "O(1)", "O(1)", [], ["transformation", "primitive"], ["canonical", "number"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00021", "Transformation Canonical Boolean", "transformation.canonical_boolean", "1.0.0", "Converts boolean tokens into a canonical boolean representation.", CalculationOperationPrimitiveFamily.Transformation, CalculationOperationCapabilityCategory.Transformation, CalculationOperationStability.Experimental, "O(1)", "O(1)", [], ["transformation", "primitive"], ["canonical", "boolean"], CalculationOperationCompatibilityStatus.Experimental)
        ];
    }
}

internal sealed class ValidationCalculationOperationDescriptorSource : ICalculationOperationDescriptorSource
{
    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        return
        [
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00022", "Validation Threshold", "validation.threshold", "1.0.0", "Validates a value against an explicit threshold relation.", CalculationOperationPrimitiveFamily.Validation, CalculationOperationCapabilityCategory.Validation, CalculationOperationStability.Fundamental, "O(1)", "O(1)", [], ["validation", "primitive"], ["threshold", "bound"]),
            CalculationOperationDescriptorSourceFactory.CreatePrimitive("ce-op-00023", "Validation Range", "validation.range", "1.0.0", "Validates whether a value lies inside an explicit range.", CalculationOperationPrimitiveFamily.Validation, CalculationOperationCapabilityCategory.Validation, CalculationOperationStability.Fundamental, "O(1)", "O(1)", [], ["validation", "primitive"], ["range", "bound"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00024", "Consumption Estimation Composite", "estimation.consumption", "1.0.0", "Composes frozen Level 1 primitives to derive baseline and occupancy-adjusted consumption estimates.", CalculationOperationPrimitiveFamily.Aggregation, CalculationOperationCapabilityCategory.Aggregation, CalculationOperationStability.Stable, "O(n)", "O(1)", ["aggregation.mean", "aggregation.weighted_mean", "normalization.ratio", "interpolation.weighted_blend", "normalization.clamp"], ["aggregation", "composite"], ["consumption", "estimation"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00025", "Forecast Projection Composite", "forecast.projection", "1.0.0", "Composes frozen Level 1 projection primitives to produce trend-based forecast outputs.", CalculationOperationPrimitiveFamily.Projection, CalculationOperationCapabilityCategory.Projection, CalculationOperationStability.Stable, "O(1)", "O(1)", ["normalization.ratio", "projection.trend_factor", "projection.threshold_variance"], ["projection", "composite"], ["forecast", "projection"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00026", "Confidence Composite", "scoring.confidence_composite", "1.0.0", "Composes spread, clamp, and confidence primitives to produce bounded confidence output.", CalculationOperationPrimitiveFamily.Scoring, CalculationOperationCapabilityCategory.Scoring, CalculationOperationStability.Stable, "O(n)", "O(1)", ["statistics.spread", "normalization.clamp", "scoring.confidence"], ["scoring", "composite"], ["confidence", "composite"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00027", "Scenario Score Composite", "scoring.scenario", "1.0.0", "Composes weighted scoring and clamp primitives to produce scenario scores.", CalculationOperationPrimitiveFamily.Scoring, CalculationOperationCapabilityCategory.Scoring, CalculationOperationStability.Stable, "O(n)", "O(1)", ["scoring.weighted_score", "normalization.clamp"], ["scoring", "composite"], ["scenario", "score"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00028", "Scenario Ranking Composite", "ranking.scenario", "1.0.0", "Composes ranking primitives to produce deterministic ranked scenario collections.", CalculationOperationPrimitiveFamily.Ranking, CalculationOperationCapabilityCategory.Ranking, CalculationOperationStability.Stable, "O(n log n)", "O(1)", ["ranking.order", "ranking.tie_break", "ranking.top_n"], ["ranking", "composite"], ["scenario", "ranking"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00029", "Canonical Import Conversion Composite", "transformation.import_canonical", "1.0.0", "Composes canonical conversion and range validation primitives for import normalization.", CalculationOperationPrimitiveFamily.Transformation, CalculationOperationCapabilityCategory.Transformation, CalculationOperationStability.Experimental, "O(n)", "O(1)", ["transformation.canonical_date", "transformation.canonical_number", "transformation.canonical_boolean", "validation.range"], ["transformation", "composite"], ["import", "canonical"]),
            CalculationOperationDescriptorSourceFactory.CreateComposite("ce-op-00030", "Pagination Composite", "validation.pagination", "1.0.0", "Composes bounds guarding and ratio primitives to derive safe pagination metadata.", CalculationOperationPrimitiveFamily.Validation, CalculationOperationCapabilityCategory.Validation, CalculationOperationStability.Fundamental, "O(1)", "O(1)", ["normalization.bounds_guard", "normalization.ratio"], ["validation", "composite"], ["pagination", "bounds"], CalculationOperationCompatibilityStatus.Obsolete)
        ];
    }
}
