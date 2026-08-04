using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Support;

/// <summary>
/// Coordinates platform framework interactions for property application operations.
/// </summary>
public interface IPropertyPlatformOrchestrator
{
    void OnPropertyMutated(Property property, string operationName);
}
