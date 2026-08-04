using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Domain.Policies;

/// <summary>
/// Centralizes reusable domain policy checks for property orchestration.
/// </summary>
public static class PropertyPolicies
{
    public static bool CanArchive(Property property)
    {
        ArgumentNullException.ThrowIfNull(property);
        return property.Units.Count == 0;
    }

    public static bool CanCreateUnit(Property property)
    {
        ArgumentNullException.ThrowIfNull(property);
        return property.Status != PropertyStatus.Archived;
    }
}
