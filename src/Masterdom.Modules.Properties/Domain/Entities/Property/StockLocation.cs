using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

public sealed class StockLocation : Entity<StockLocationId>
{
    internal StockLocation(StockLocationId id, string name, string? code)
        : base(id)
    {
        Name = name;
        Code = code;
        IsActive = true;
    }

    public string Name { get; private set; }

    public string? Code { get; private set; }

    public bool IsActive { get; private set; }

    public PropertyId PropertyId { get; private set; } = default!;

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    internal void AttachToProperty(PropertyId propertyId)
    {
        if (PropertyId != default && PropertyId != propertyId)
            throw new InvalidOperationException("A stock location cannot be reassigned to a different property.");
        PropertyId = propertyId;
    }
}
