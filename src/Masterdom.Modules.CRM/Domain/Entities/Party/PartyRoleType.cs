using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the business role performed by a party.
/// </summary>
public sealed class PartyRoleType : ValueObject
{
    public static readonly PartyRoleType Tenant = new("Tenant");
    public static readonly PartyRoleType PropertyOwner = new("PropertyOwner");
    public static readonly PartyRoleType Vendor = new("Vendor");
    public static readonly PartyRoleType Contractor = new("Contractor");
    public static readonly PartyRoleType Employee = new("Employee");
    public static readonly PartyRoleType Broker = new("Broker");
    public static readonly PartyRoleType EmergencyContact = new("EmergencyContact");
    public static readonly PartyRoleType UtilityProvider = new("UtilityProvider");
    public static readonly PartyRoleType TelecomProvider = new("TelecomProvider");
    public static readonly PartyRoleType GovernmentAuthority = new("GovernmentAuthority");
    public static readonly PartyRoleType Society = new("Society");
    public static readonly PartyRoleType LegalAdvisor = new("LegalAdvisor");
    public static readonly PartyRoleType Accountant = new("Accountant");
    public static readonly PartyRoleType PropertyManager = new("PropertyManager");

    private PartyRoleType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PartyRoleType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "TENANT" => Tenant,
            "PROPERTYOWNER" => PropertyOwner,
            "VENDOR" => Vendor,
            "CONTRACTOR" => Contractor,
            "EMPLOYEE" => Employee,
            "BROKER" => Broker,
            "EMERGENCYCONTACT" => EmergencyContact,
            "UTILITYPROVIDER" => UtilityProvider,
            "TELECOMPROVIDER" => TelecomProvider,
            "GOVERNMENTAUTHORITY" => GovernmentAuthority,
            "SOCIETY" => Society,
            "LEGALADVISOR" => LegalAdvisor,
            "ACCOUNTANT" => Accountant,
            "PROPERTYMANAGER" => PropertyManager,
            _ => new PartyRoleType(value)
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
