using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.IdentityProfile;

/// <summary>
/// Represents the unique identifier of an identity profile.
/// </summary>
public sealed record IdentityProfileId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new identity profile identifier.
    /// </summary>
    public static IdentityProfileId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates an identity profile identifier from an existing Guid.
    /// </summary>
    public static IdentityProfileId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "IdentityProfileId cannot be empty.",
                nameof(value));

        return new(value);
    }

    /// <summary>
    /// Parses a string into an identity profile identifier.
    /// </summary>
    public static IdentityProfileId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
