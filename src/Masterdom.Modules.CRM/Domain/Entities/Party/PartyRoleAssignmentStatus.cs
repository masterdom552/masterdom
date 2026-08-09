using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the lifecycle status of a party role assignment.
/// </summary>
public sealed class PartyRoleAssignmentStatus : ValueObject
{
    public static readonly PartyRoleAssignmentStatus Active = new("Active");
    public static readonly PartyRoleAssignmentStatus Inactive = new("Inactive");
    public static readonly PartyRoleAssignmentStatus Removed = new("Removed");

    private PartyRoleAssignmentStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PartyRoleAssignmentStatus Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "ACTIVE" => Active,
            "INACTIVE" => Inactive,
            "REMOVED" => Removed,
            _ => new PartyRoleAssignmentStatus(value)
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
