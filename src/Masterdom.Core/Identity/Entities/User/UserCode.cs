using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.User;

/// <summary>
/// Represents the business code of a user.
/// </summary>
public sealed class UserCode : ValueObject
{
    private UserCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the user code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a user code.
    /// </summary>
    public static UserCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException(
                "User code cannot exceed 50 characters.",
                nameof(value));
        }

        return new UserCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(UserCode code)
    {
        return code.Value;
    }
}
