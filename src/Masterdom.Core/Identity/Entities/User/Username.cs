using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.User;

/// <summary>
/// Represents a username used for authentication.
/// </summary>
public sealed class Username : ValueObject
{
    private Username(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the username.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a username.
    /// </summary>
    public static Username Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        if (value.Length < 3)
        {
            throw new ArgumentException(
                "Username must be at least 3 characters long.",
                nameof(value));
        }

        if (value.Length > 100)
        {
            throw new ArgumentException(
                "Username cannot exceed 100 characters.",
                nameof(value));
        }

        if (value.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException(
                "Username cannot contain whitespace.",
                nameof(value));
        }

        return new Username(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Username username)
    {
        return username.Value;
    }
}
