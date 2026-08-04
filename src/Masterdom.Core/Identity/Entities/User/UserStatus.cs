using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.User;

/// <summary>
/// Represents the lifecycle status of a user.
/// </summary>
public sealed class UserStatus : ValueObject
{
    public static readonly UserStatus Active = new("Active");
    public static readonly UserStatus Inactive = new("Inactive");
    public static readonly UserStatus Locked = new("Locked");
    public static readonly UserStatus Suspended = new("Suspended");
    public static readonly UserStatus Archived = new("Archived");

    private UserStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the user status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a user status.
    /// </summary>
    public static UserStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "LOCKED" => Locked,
            "SUSPENDED" => Suspended,
            "ARCHIVED" => Archived,
            _ => new UserStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
