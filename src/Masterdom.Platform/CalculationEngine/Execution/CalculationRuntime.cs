using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal sealed class CalculationRuntime : ICalculationRuntime
{
    private readonly ICalculationEngine _engine;
    private readonly CalculationOperationRegistry _metadataRegistry;

    internal CalculationRuntime()
        : this(CalculationEngineFactory.CreateDefault(), new CalculationOperationRegistry())
    {
    }

    internal CalculationRuntime(
        ICalculationEngine engine,
        CalculationOperationRegistry metadataRegistry)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _metadataRegistry = metadataRegistry ?? throw new ArgumentNullException(nameof(metadataRegistry));
    }

    public ICalculationResult Execute(CalculationRuntimeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var descriptor = _metadataRegistry.ResolveByCapabilityId(
            CalculationOperationCapabilityId.Create(request.CapabilityId.Value));
        var executionRequest = new CalculationRequest(
            descriptor.DescriptorId,
            request.Context,
            request.Input);

        return _engine.Execute(executionRequest);
    }
}
