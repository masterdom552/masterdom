using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ExternalLogin;

/// <summary>
/// Represents the unique identifier of an external login.
/// </summary>
public sealed record ExternalLoginId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new external login identifier.
    /// </summary>
    public static ExternalLoginId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates an external login identifier from an existing Guid.
    /// </summary>
    public static ExternalLoginId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "ExternalLoginId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into an external login identifier.
    /// </summary>
    public static ExternalLoginId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
