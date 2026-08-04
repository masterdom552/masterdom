using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.IdentityProfile;

/// <summary>
/// Represents the business code of an identity profile.
/// </summary>
public sealed class IdentityProfileCode : ValueObject
{
    private IdentityProfileCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the identity profile code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an identity profile code.
    /// </summary>
    public static IdentityProfileCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException(
                "Identity profile code cannot exceed 50 characters.",
                nameof(value));
        }

        return new IdentityProfileCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(IdentityProfileCode code)
    {
        return code.Value;
    }
}
