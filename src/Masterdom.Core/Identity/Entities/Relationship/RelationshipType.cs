using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Relationship;

/// <summary>
/// Represents the type of relationship between two identity profiles.
/// </summary>
public sealed class RelationshipType : ValueObject
{
    public static readonly RelationshipType Owner = new("Owner");
    public static readonly RelationshipType Tenant = new("Tenant");
    public static readonly RelationshipType Landlord = new("Landlord");
    public static readonly RelationshipType PropertyManager = new("Property Manager");
    public static readonly RelationshipType Employee = new("Employee");
    public static readonly RelationshipType Employer = new("Employer");
    public static readonly RelationshipType Vendor = new("Vendor");
    public static readonly RelationshipType Customer = new("Customer");
    public static readonly RelationshipType Contractor = new("Contractor");
    public static readonly RelationshipType ServiceProvider = new("Service Provider");
    public static readonly RelationshipType UtilityProvider = new("Utility Provider");
    public static readonly RelationshipType TelecomOperator = new("Telecom Operator");
    public static readonly RelationshipType Parent = new("Parent");
    public static readonly RelationshipType Child = new("Child");
    public static readonly RelationshipType Spouse = new("Spouse");
    public static readonly RelationshipType Sibling = new("Sibling");
    public static readonly RelationshipType Guardian = new("Guardian");
    public static readonly RelationshipType Nominee = new("Nominee");
    public static readonly RelationshipType EmergencyContact = new("Emergency Contact");
    public static readonly RelationshipType AuthorizedSignatory = new("Authorized Signatory");
    public static readonly RelationshipType Other = new("Other");

    private RelationshipType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the relationship type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a relationship type.
    /// </summary>
    public static RelationshipType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "OWNER" => Owner,
            "TENANT" => Tenant,
            "LANDLORD" => Landlord,
            "PROPERTY MANAGER" => PropertyManager,
            "EMPLOYEE" => Employee,
            "EMPLOYER" => Employer,
            "VENDOR" => Vendor,
            "CUSTOMER" => Customer,
            "CONTRACTOR" => Contractor,
            "SERVICE PROVIDER" => ServiceProvider,
            "UTILITY PROVIDER" => UtilityProvider,
            "TELECOM OPERATOR" => TelecomOperator,
            "PARENT" => Parent,
            "CHILD" => Child,
            "SPOUSE" => Spouse,
            "SIBLING" => Sibling,
            "GUARDIAN" => Guardian,
            "NOMINEE" => Nominee,
            "EMERGENCY CONTACT" => EmergencyContact,
            "AUTHORIZED SIGNATORY" => AuthorizedSignatory,
            "OTHER" => Other,
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
