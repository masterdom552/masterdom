using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the unique identifier of a party.
/// </summary>
public sealed record PartyId(Guid Value) : EntityId(Value)
{
    public static PartyId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static PartyId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PartyId cannot be empty.", nameof(value));
        }

        return new(value);
    }

    public static PartyId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return From(Guid.Parse(value));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
