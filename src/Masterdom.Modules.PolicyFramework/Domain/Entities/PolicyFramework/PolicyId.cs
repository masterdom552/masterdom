using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed record PolicyId(Guid Value) : EntityId(Value)
{
    public static PolicyId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static PolicyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PolicyId cannot be empty.", nameof(value));
        }

        return new(value);
    }
}
