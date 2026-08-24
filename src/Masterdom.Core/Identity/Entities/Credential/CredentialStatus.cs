using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Credential;

/// <summary>
/// Represents the lifecycle status of a credential.
/// </summary>
public sealed class CredentialStatus : ValueObject
{
    public static readonly CredentialStatus Active = new("Active");
    public static readonly CredentialStatus Revoked = new("Revoked");

    private CredentialStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CredentialStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "REVOKED" => Revoked,
            _ => new CredentialStatus(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}
