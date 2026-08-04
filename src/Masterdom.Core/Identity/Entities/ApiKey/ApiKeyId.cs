using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.ApiKey;

/// <summary>
/// Represents the unique identifier of an API key.
/// </summary>
public sealed record ApiKeyId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new API key identifier.
    /// </summary>
    public static ApiKeyId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates an API key identifier from an existing Guid.
    /// </summary>
    public static ApiKeyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException(
                "ApiKeyId cannot be empty.",
                nameof(value));
        }

        return new(value);
    }

    /// <summary>
    /// Parses a string into an API key identifier.
    /// </summary>
    public static ApiKeyId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
