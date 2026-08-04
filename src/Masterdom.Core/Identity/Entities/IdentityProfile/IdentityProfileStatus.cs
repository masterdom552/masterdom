using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.IdentityProfile;

/// <summary>
/// Represents the lifecycle status of an identity profile.
/// </summary>
public sealed class IdentityProfileStatus : ValueObject
{
    public static readonly IdentityProfileStatus Active = new("Active");
    public static readonly IdentityProfileStatus Inactive = new("Inactive");
    public static readonly IdentityProfileStatus Archived = new("Archived");

    private IdentityProfileStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the status value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an identity profile status.
    /// </summary>
    public static IdentityProfileStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new IdentityProfileStatus(value)
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
