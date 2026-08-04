using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents internal notes for a person.
/// </summary>
public sealed class Notes : ValueObject
{
    private Notes(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Notes Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new Notes(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
