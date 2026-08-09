using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

public sealed record StockLocationId(Guid Value) : EntityId(Value)
{
    public static StockLocationId New() => new(Guid.CreateVersion7());

    public static StockLocationId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StockLocationId cannot be empty.", nameof(value));
        return new(value);
    }
}
