using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.UserRole;

public sealed class UserRoleStatus : ValueObject
{
    public static readonly UserRoleStatus Active = new("Active");
    public static readonly UserRoleStatus Inactive = new("Inactive");
    public static readonly UserRoleStatus Archived = new("Archived");

    private UserRoleStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static UserRoleStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new UserRoleStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}
