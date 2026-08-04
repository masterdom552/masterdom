using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents the unique identifier of a bill.
/// </summary>
public sealed record BillId(Guid Value) : EntityId(Value)
{
    public static BillId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static BillId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BillId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
