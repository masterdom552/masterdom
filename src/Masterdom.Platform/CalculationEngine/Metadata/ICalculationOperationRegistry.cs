using System.Collections.Generic;

namespace Masterdom.Platform.CalculationEngine.Metadata;

/// <summary>
/// Registers and resolves calculation operation metadata.
/// </summary>
public interface ICalculationOperationRegistry
{
    IReadOnlyCollection<ICalculationOperationDescriptor> GetAll();

    ICalculationOperationDescriptor ResolveByDescriptorId(CalculationOperationDescriptorId descriptorId);

    ICalculationOperationDescriptor ResolveByCapabilityId(CalculationOperationCapabilityId capabilityId);

    ICalculationOperationDescriptor ResolveByOperationName(string operationName);

    IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByPrimitiveFamily(CalculationOperationPrimitiveFamily primitiveFamily);

    IReadOnlyCollection<ICalculationOperationDescriptor> GetByCapabilityCategory(CalculationOperationCapabilityCategory capabilityCategory);

    IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByCompositionLevel(CalculationOperationCompositionLevel compositionLevel);

    IReadOnlyCollection<ICalculationOperationDescriptor> ResolveByCompatibilityStatus(CalculationOperationCompatibilityStatus compatibilityStatus);
}
