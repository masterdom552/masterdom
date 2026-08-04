namespace Masterdom.Platform.CalculationEngine.Metadata;

internal interface ICalculationOperationDiscoveryStrategy
{
    IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors();
}

internal interface ICompositeCalculationOperationDiscoveryStrategy
    : ICalculationOperationDiscoveryStrategy
{
}
