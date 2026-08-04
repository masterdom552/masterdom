using CalcComposites = Masterdom.Platform.CalculationEngine.
    Composites;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal static class CalculationRuntimeExecutionRegistryBuilder
{
    internal static ICalculationExecutionRegistry CreateDefault()
    {
        var metadataRegistry = new CalculationOperationRegistry();
        var primitiveRegistry = CalculationPrimitiveExecutionRegistryBuilder.CreateDefault();
        var primitiveEngine = new CalculationExecutionPipeline(
            new CalculationRequestValidator(),
            new CalculationOperationResolver(primitiveRegistry),
            new CalculationExecutor(),
            new CalculationResultValidator());

        var primitiveExecutor = new CalcComposites.CompositePrimitiveExecutor(primitiveEngine, metadataRegistry);
        var compositeOperations = CreateCompositeOperations(primitiveExecutor);

        var definitions = CreateDefinitions(CalculationPrimitiveExecutionRegistryBuilder.GetRegisteredOperations(), metadataRegistry)
            .Concat(CreateDefinitions(compositeOperations, metadataRegistry))
            .ToArray();

        return new CalculationExecutionRegistry(definitions);
    }

    private static IReadOnlyDictionary<string, ICalculationComposite> CreateCompositeOperations(CalcComposites.CompositePrimitiveExecutor primitiveExecutor)
    {
        ArgumentNullException.ThrowIfNull(primitiveExecutor);

        return new Dictionary<string, ICalculationComposite>(StringComparer.OrdinalIgnoreCase)
        {
            [CalcComposites.CompositeCapabilityIds.ConsumptionEstimation] = new ConsumptionEstimationCompositeOperation(new CalcComposites.ConsumptionEstimationCompositeCalculator(primitiveExecutor)),
            [CalcComposites.CompositeCapabilityIds.ForecastProjection] = new ForecastProjectionCompositeOperation(new CalcComposites.ForecastProjectionCompositeCalculator(primitiveExecutor)),
            [CalcComposites.CompositeCapabilityIds.Confidence] = new ConfidenceCompositeOperation(new CalcComposites.ConfidenceCompositeCalculator(primitiveExecutor)),
            [CalcComposites.CompositeCapabilityIds.ScenarioScore] = new ScenarioScoreCompositeOperation(new CalcComposites.ScenarioScoreCompositeCalculator(primitiveExecutor)),
            [CalcComposites.CompositeCapabilityIds.ScenarioRanking] = new ScenarioRankingCompositeOperation(new CalcComposites.ScenarioRankingCompositeCalculator(primitiveExecutor)),
            [CalcComposites.CompositeCapabilityIds.CanonicalImportConversion] = new CanonicalImportConversionCompositeOperation(new CalcComposites.CanonicalImportConversionCompositeCalculator(primitiveExecutor)),
            [CalcComposites.CompositeCapabilityIds.Pagination] = new PaginationCompositeOperation(new CalcComposites.PaginationCompositeCalculator(primitiveExecutor))
        };
    }

    private static IReadOnlyCollection<CalculationOperationExecutionDefinition> CreateDefinitions<TOperation>(
        IReadOnlyDictionary<string, TOperation> operations,
        ICalculationOperationRegistry metadataRegistry)
        where TOperation : ICalculationOperation
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(metadataRegistry);

        var descriptors = metadataRegistry
            .GetAll()
            .Where(descriptor => operations.ContainsKey(descriptor.CapabilityId.Value))
            .ToArray();

        var missingDescriptors = operations.Keys
            .Where(capabilityId => descriptors.All(descriptor =>
                !string.Equals(descriptor.CapabilityId.Value, capabilityId, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        if (missingDescriptors.Length > 0)
        {
            throw new CalculationOperationValidationException(
                $"Runtime descriptors are missing for capability ids: {string.Join(", ", missingDescriptors)}.");
        }

        return descriptors
            .Select(descriptor =>
                new CalculationOperationExecutionDefinition(
                    operations[descriptor.CapabilityId.Value],
                    descriptor))
            .ToArray();
    }
}
