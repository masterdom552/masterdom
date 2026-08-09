using Masterdom.Core.Primitives;

namespace Masterdom.Modules.CRM.Domain.Entities.Party;

/// <summary>
/// Represents the type of relationship between two parties.
/// </summary>
public sealed class RelationshipType : ValueObject
{
    public static readonly RelationshipType Owns = new("owns");
    public static readonly RelationshipType Manages = new("manages");
    public static readonly RelationshipType WorksFor = new("works_for");
    public static readonly RelationshipType GuarantorOf = new("guarantor_of");
    public static readonly RelationshipType EmergencyContact = new("emergency_contact");
    public static readonly RelationshipType SupplierOf = new("supplier_of");
    public static readonly RelationshipType TenantOf = new("tenant_of");
    public static readonly RelationshipType ContractorFor = new("contractor_for");

    private RelationshipType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static RelationshipType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "OWNS" => Owns,
            "MANAGES" => Manages,
            "WORKS_FOR" => WorksFor,
            "GUARANTOR_OF" => GuarantorOf,
            "EMERGENCY_CONTACT" => EmergencyContact,
            "SUPPLIER_OF" => SupplierOf,
            "TENANT_OF" => TenantOf,
            "CONTRACTOR_FOR" => ContractorFor,
            _ => new RelationshipType(value)
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
