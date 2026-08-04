using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Permission;

/// <summary>
/// Represents the lifecycle status of a permission.
/// </summary>
public sealed class PermissionStatus : ValueObject
{
    public static readonly PermissionStatus Active = new("Active");
    public static readonly PermissionStatus Inactive = new("Inactive");
    public static readonly PermissionStatus Archived = new("Archived");

    private PermissionStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PermissionStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new PermissionStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}
