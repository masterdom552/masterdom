namespace Masterdom.Core.Security;

/// <summary>
/// Defines authority levels in the Masterdom hierarchy.
/// </summary>
public static class AuthorityLevels
{
    /// <summary>
    /// Primary SuperUser: Unrestricted platform access and delegation authority.
    /// </summary>
    public const int PrimarySuperUser = 4;

    /// <summary>
    /// Secondary SuperUser: Delegated authority, bounded by Primary's delegation.
    /// </summary>
    public const int SecondarySuperUser = 3;

    /// <summary>
    /// Admin: Delegated authority from Secondary or Primary, cannot delegate further.
    /// </summary>
    public const int Admin = 2;

    /// <summary>
    /// Tenant: Limited self-access and property-scoped operations.
    /// </summary>
    public const int Tenant = 1;

    /// <summary>
    /// Gets the maximum delegation depth supported in this package.
    /// </summary>
    public const int MaxDelegationDepth = 2;

    /// <summary>
    /// Determines whether a level can delegate authority to other users.
    /// </summary>
    public static bool CanDelegate(int authorityLevel)
    {
        return authorityLevel >= SecondarySuperUser;
    }

    /// <summary>
    /// Determines whether a child authority level is valid for the given parent level.
    /// </summary>
    public static bool IsValidChild(int parentLevel, int childLevel)
    {
        // Parent must be at least as high as child
        return parentLevel >= childLevel;
    }
}
