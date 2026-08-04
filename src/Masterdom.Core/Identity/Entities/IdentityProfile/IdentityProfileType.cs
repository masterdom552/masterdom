using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.IdentityProfile;

/// <summary>
/// Represents the type of an identity profile.
/// </summary>
public sealed class IdentityProfileType : ValueObject
{
    public static readonly IdentityProfileType Person = new("Person");
    public static readonly IdentityProfileType Organization = new("Organization");
    public static readonly IdentityProfileType Owner = new("Owner");
    public static readonly IdentityProfileType Tenant = new("Tenant");
    public static readonly IdentityProfileType Employee = new("Employee");
    public static readonly IdentityProfileType Vendor = new("Vendor");
    public static readonly IdentityProfileType Contractor = new("Contractor");
    public static readonly IdentityProfileType ServiceProvider = new("Service Provider");
    public static readonly IdentityProfileType GovernmentAgency = new("Government Agency");
    public static readonly IdentityProfileType FinancialInstitution = new("Financial Institution");
    public static readonly IdentityProfileType UtilityProvider = new("Utility Provider");
    public static readonly IdentityProfileType TelecomOperator = new("Telecom Operator");
    public static readonly IdentityProfileType Other = new("Other");

    private IdentityProfileType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the profile type.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates an identity profile type.
    /// </summary>
    public static IdentityProfileType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        value = value.Trim();

        return value.ToUpperInvariant() switch
        {
            "PERSON" => Person,
            "ORGANIZATION" => Organization,
            "OWNER" => Owner,
            "TENANT" => Tenant,
            "EMPLOYEE" => Employee,
            "VENDOR" => Vendor,
            "CONTRACTOR" => Contractor,
            "SERVICE PROVIDER" => ServiceProvider,
            "GOVERNMENT AGENCY" => GovernmentAgency,
            "FINANCIAL INSTITUTION" => FinancialInstitution,
            "UTILITY PROVIDER" => UtilityProvider,
            "TELECOM OPERATOR" => TelecomOperator,
            "OTHER" => Other,
            _ => new IdentityProfileType(value)
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
