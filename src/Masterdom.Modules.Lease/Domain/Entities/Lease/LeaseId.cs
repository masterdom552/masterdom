using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Lease.Domain.Entities.Lease;

/// <summary>
/// Represents the unique identifier of a lease.
/// </summary>
public sealed record LeaseId(Guid Value) : EntityId(Value)
{
    public static LeaseId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static LeaseId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("LeaseId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
