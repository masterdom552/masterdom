using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Metadata;

internal sealed class CompositeCalculationOperationDiscoveryStrategy : ICompositeCalculationOperationDiscoveryStrategy
{
    private readonly ImmutableArray<ICalculationOperationDiscoveryStrategy> _strategies;

    public CompositeCalculationOperationDiscoveryStrategy()
        : this([new ReflectionCalculationOperationDiscoveryStrategy()])
    {
    }

    public CompositeCalculationOperationDiscoveryStrategy(IEnumerable<ICalculationOperationDiscoveryStrategy> strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);

        _strategies = strategies
            .OrderBy(strategy => strategy.GetType().FullName, StringComparer.Ordinal)
            .ToImmutableArray();
    }

    public IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors()
    {
        var descriptors = _strategies
            .SelectMany(strategy => strategy.GetDescriptors())
            .ToArray();

        return ImmutableArray.CreateRange(descriptors);
    }
}
