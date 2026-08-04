using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ExternalLogin;

/// <summary>
/// Represents the lifecycle status of an external login.
/// </summary>
public sealed class ExternalLoginStatus : ValueObject
{
    public static readonly ExternalLoginStatus Active = new("Active");
    public static readonly ExternalLoginStatus Inactive = new("Inactive");
    public static readonly ExternalLoginStatus Archived = new("Archived");

    private ExternalLoginStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the external login status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an external login status.
    /// </summary>
    public static ExternalLoginStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "ARCHIVED" => Archived,
            _ => new ExternalLoginStatus(value)
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
