using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents property reference for billing ownership boundary.
/// </summary>
public sealed class PropertyReference : ValueObject
{
    private PropertyReference(Guid propertyId)
    {
        PropertyId = propertyId;
    }

    public Guid PropertyId { get; }

    public static PropertyReference Create(Guid propertyId)
    {
        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("Property reference cannot be empty.", nameof(propertyId));
        }

        return new PropertyReference(propertyId);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PropertyId;
    }
}
