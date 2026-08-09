using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the unique identifier of a party role assignment.
/// </summary>
public sealed record PartyRoleAssignmentId(Guid Value) : EntityId(Value)
{
    public static PartyRoleAssignmentId New()
    {
        return new(Guid.CreateVersion7());
    }

    public static PartyRoleAssignmentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PartyRoleAssignmentId cannot be empty.", nameof(value));
        }

        return new(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
