using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents the business code of a person.
/// </summary>
public sealed class PersonCode : ValueObject
{
    private PersonCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the person code.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a person code.
    /// </summary>
    public static PersonCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            throw new ArgumentException(
                "Person code cannot exceed 50 characters.",
                nameof(value));
        }

        return new PersonCode(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(PersonCode code)
    {
        return code.Value;
    }
}
