using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents the unique identifier of a tenancy.
/// </summary>
public sealed record TenancyId(Guid Value) : EntityId(Value)
{
    public static TenancyId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static TenancyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("TenancyId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
