using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's nationality.
/// </summary>
public sealed class Nationality : ValueObject
{
    private Nationality(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Nationality Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new Nationality(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
