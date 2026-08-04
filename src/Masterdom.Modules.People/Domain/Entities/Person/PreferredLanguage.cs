using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's preferred language.
/// </summary>
public sealed class PreferredLanguage : ValueObject
{
    private PreferredLanguage(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PreferredLanguage Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new PreferredLanguage(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
