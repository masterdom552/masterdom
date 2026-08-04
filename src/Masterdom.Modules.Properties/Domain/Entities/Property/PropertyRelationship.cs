using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents a relationship edge from one property to another property.
/// </summary>
public sealed class PropertyRelationship : ValueObject
{
    public PropertyRelationship(PropertyId targetPropertyId, PropertyRelationshipType type)
    {
        ArgumentNullException.ThrowIfNull(targetPropertyId);

        TargetPropertyId = targetPropertyId;
        Type = type;
    }

    public PropertyId TargetPropertyId { get; }

    public PropertyRelationshipType Type { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TargetPropertyId;
        yield return Type;
    }
}
