using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ApiKey;

/// <summary>
/// Represents the lifecycle status of an API key.
/// </summary>
public sealed class ApiKeyStatus : ValueObject
{
    public static readonly ApiKeyStatus Active = new("Active");
    public static readonly ApiKeyStatus Inactive = new("Inactive");
    public static readonly ApiKeyStatus Expired = new("Expired");
    public static readonly ApiKeyStatus Revoked = new("Revoked");
    public static readonly ApiKeyStatus Archived = new("Archived");

    private ApiKeyStatus(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the API key status.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an API key status.
    /// </summary>
    public static ApiKeyStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "EXPIRED" => Expired,
            "REVOKED" => Revoked,
            "ARCHIVED" => Archived,
            _ => new ApiKeyStatus(value)
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
