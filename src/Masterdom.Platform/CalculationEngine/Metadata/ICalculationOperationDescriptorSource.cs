namespace Masterdom.Platform.CalculationEngine.Metadata;

internal interface ICalculationOperationDescriptorSource
{
    IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors();
}
