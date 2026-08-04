using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identifiers;

/// <summary>
/// Represents the unique identifier of a person.
/// </summary>
public sealed record PersonId(Guid Value) : EntityId(Value)
{
    /// <summary>
    /// Creates a new person identifier.
    /// </summary>
    public static PersonId New()
    {
        return new(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a person identifier from an existing Guid.
    /// </summary>
    public static PersonId From(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(
                "PersonId cannot be empty.",
                nameof(value));

        return new(value);
    }

    /// <summary>
    /// Parses a string into a person identifier.
    /// </summary>
    public static PersonId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
