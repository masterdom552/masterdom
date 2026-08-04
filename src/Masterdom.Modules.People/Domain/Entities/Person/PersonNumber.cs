using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents the business identity number of a person.
/// </summary>
public sealed class PersonNumber : ValueObject
{
    private PersonNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the identity number.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an identity number.
    /// </summary>
    public static PersonNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 50)
        {
            throw new ArgumentException("Person number cannot exceed 50 characters.", nameof(value));
        }

        return new PersonNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
