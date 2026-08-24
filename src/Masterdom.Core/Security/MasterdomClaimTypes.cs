namespace Masterdom.Core.Security;

/// <summary>
/// Defines custom claim types used by the Masterdom request pipeline.
/// </summary>
public static class MasterdomClaimTypes
{
    public const string Permission = "masterdom:permission";
    public const string PersonId = "masterdom:person_id";
    public const string PropertyScope = "masterdom:property_scope";
    public const string OwnedProperty = "masterdom:owned_property";
    public const string AuthorityLevel = "masterdom:authority_level";
}
