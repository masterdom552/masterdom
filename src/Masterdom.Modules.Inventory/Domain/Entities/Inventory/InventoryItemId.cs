using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Inventory.Domain.Entities.Inventory;

public sealed record InventoryItemId(Guid Value) : EntityId(Value)
{
    public static InventoryItemId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static InventoryItemId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("InventoryItemId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
