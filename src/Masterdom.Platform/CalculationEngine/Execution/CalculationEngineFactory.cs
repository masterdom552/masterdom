using Masterdom.Platform.CalculationEngine.Contracts;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal static class CalculationEngineFactory
{
    internal static ICalculationEngine CreateDefault()
    {
        var registry = CalculationRuntimeExecutionRegistryBuilder.CreateDefault();

        return new CalculationExecutionPipeline(
            new CalculationRequestValidator(),
            new CalculationOperationResolver(registry),
            new CalculationExecutor(),
            new CalculationResultValidator());
    }
}
