namespace Masterdom.Platform.CalculationEngine.Metadata;

internal interface ICalculationOperationDescriptorProvider
{
    IReadOnlyCollection<ICalculationOperationDescriptor> GetDescriptors();
}
