using Masterdom.Core.Primitives;

namespace Masterdom.Modules.People.Domain.Entities.Person;

/// <summary>
/// Represents a person's gender.
/// </summary>
public sealed class Gender : ValueObject
{
    public static readonly Gender Male = new("Male");
    public static readonly Gender Female = new("Female");
    public static readonly Gender NonBinary = new("Non-Binary");
    public static readonly Gender Other = new("Other");
    public static readonly Gender PreferNotToSay = new("Prefer Not To Say");

    private Gender(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the gender value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a gender.
    /// </summary>
    public static Gender Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "MALE" => Male,
            "FEMALE" => Female,
            "NON-BINARY" => NonBinary,
            "NON BINARY" => NonBinary,
            "OTHER" => Other,
            "PREFER NOT TO SAY" => PreferNotToSay,
            _ => new Gender(value)
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }
}
