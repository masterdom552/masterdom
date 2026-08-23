namespace Masterdom.Core.Identity.Entities.DelegatedAuthority;

/// <summary>
/// Represents the lifecycle status of a delegated authority.
/// </summary>
public enum DelegatedAuthorityStatus
{
    /// <summary>
    /// Delegation is active and effective.
    /// </summary>
    Active,

    /// <summary>
    /// Delegation has expired (EffectiveToUtc passed).
    /// </summary>
    Expired,

    /// <summary>
    /// Delegation has been revoked.
    /// </summary>
    Revoked
}
