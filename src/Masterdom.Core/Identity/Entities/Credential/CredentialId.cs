using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Credential;

/// <summary>
/// Represents the unique identifier of a credential.
/// </summary>
public sealed record CredentialId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new credential identifier.
    /// </summary>
    public static CredentialId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a credential identifier from an existing Guid.
    /// </summary>
    public static CredentialId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "CredentialId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
