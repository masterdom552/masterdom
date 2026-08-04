using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a preferred contact method for a person.
/// </summary>
public sealed class PreferredContact : ValueObject
{
    private PreferredContact(string type, string value)
    {
        Type = type;
        Value = value;
    }

    public string Type { get; }

    public string Value { get; }

    public static PreferredContact Create(string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return new PreferredContact(type.Trim(), value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type.ToUpperInvariant();
        yield return Value.ToUpperInvariant();
    }
}
