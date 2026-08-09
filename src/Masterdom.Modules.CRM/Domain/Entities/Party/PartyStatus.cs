using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the lifecycle status of a party.
/// </summary>
public sealed class PartyStatus : ValueObject
{
    public static readonly PartyStatus Active = new("Active");
    public static readonly PartyStatus Inactive = new("Inactive");

    private PartyStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PartyStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            _ => new PartyStatus(value)
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
