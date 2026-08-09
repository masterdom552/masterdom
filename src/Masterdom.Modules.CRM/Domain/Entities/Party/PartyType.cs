using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the classification of a party.
/// </summary>
public sealed class PartyType : ValueObject
{
    public static readonly PartyType Person = new("Person");
    public static readonly PartyType Organization = new("Organization");

    private PartyType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PartyType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PERSON" => Person,
            "ORGANIZATION" => Organization,
            _ => new PartyType(value)
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
