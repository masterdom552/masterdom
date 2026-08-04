using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.UserRole;

public sealed record UserRoleId(Guid Value) : EntityId(Value)
{
    public static UserRoleId New() => new(Guid.CreateVersion7());

    public static UserRoleId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserRoleId cannot be empty.", nameof(value));

        return new(value);
    }

    public override string ToString() => Value.ToString();
}
