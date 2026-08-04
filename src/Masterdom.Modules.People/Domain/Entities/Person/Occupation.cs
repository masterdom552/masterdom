using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's occupation.
/// </summary>
public sealed class Occupation : ValueObject
{
    private Occupation(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Occupation Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new Occupation(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
