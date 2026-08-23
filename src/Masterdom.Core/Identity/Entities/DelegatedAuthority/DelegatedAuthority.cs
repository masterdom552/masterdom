using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.DelegatedAuthority;

/// <summary>
/// Represents a bounded grant of authority from one user to another.
///
/// A delegated authority is distinct from a direct role assignment:
/// - Direct role: User has inherent authority from their assigned roles
/// - Delegated authority: User received authority from another user's explicit delegation
///
/// Delegated authority is temporal, scopable, and revocable independently from role assignments.
/// </summary>
public sealed class DelegatedAuthority : AggregateRoot<DelegatedAuthorityId>
{
    private DelegatedAuthority(
        DelegatedAuthorityId id,
        UserId delegatorUserId,
        UserId delegatedToUserId,
        RoleId delegatedRoleId,
        DelegationScope scope,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(delegatorUserId);
        ArgumentNullException.ThrowIfNull(delegatedToUserId);
        ArgumentNullException.ThrowIfNull(delegatedRoleId);
        ArgumentNullException.ThrowIfNull(scope);

        if (effectiveToUtc.HasValue && effectiveToUtc.Value < effectiveFromUtc)
        {
            throw new ArgumentException(
                "EffectiveToUtc cannot be earlier than EffectiveFromUtc.",
                nameof(effectiveToUtc));
        }

        DelegatorUserId = delegatorUserId;
        DelegatedToUserId = delegatedToUserId;
        DelegatedRoleId = delegatedRoleId;
        Scope = scope;
        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;

        Status = DelegatedAuthorityStatus.Active;
        CreatedAtUtc = DateTime.UtcNow;

        Description = null;
        Remarks = null;

        RevokedAtUtc = null;
        RevokedBy = null;
        RevocationReason = null;
    }

    /// <summary>
    /// Creates a new delegated authority.
    /// </summary>
    public static DelegatedAuthority Create(
        UserId delegatorUserId,
        UserId delegatedToUserId,
        RoleId delegatedRoleId,
        DelegationScope scope,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null)
    {
        var effectiveFrom = effectiveFromUtc ?? DateTime.UtcNow;

        return new DelegatedAuthority(
            DelegatedAuthorityId.New(),
            delegatorUserId,
            delegatedToUserId,
            delegatedRoleId,
            scope,
            effectiveFrom,
            effectiveToUtc);
    }

    /// <summary>
    /// Gets the user who delegated the authority.
    /// </summary>
    public UserId DelegatorUserId { get; }

    /// <summary>
    /// Gets the user who receives the delegated authority.
    /// </summary>
    public UserId DelegatedToUserId { get; }

    /// <summary>
    /// Gets the role that was delegated.
    /// </summary>
    public RoleId DelegatedRoleId { get; }

    /// <summary>
    /// Gets the scope constraints of this delegation.
    /// </summary>
    public DelegationScope Scope { get; }

    /// <summary>
    /// Gets when this delegation becomes effective.
    /// </summary>
    public DateTime EffectiveFromUtc { get; }

    /// <summary>
    /// Gets when this delegation expires (if set).
    /// </summary>
    public DateTime? EffectiveToUtc { get; }

    /// <summary>
    /// Gets the current status of the delegation.
    /// </summary>
    public DelegatedAuthorityStatus Status { get; private set; }

    /// <summary>
    /// Gets when this delegation was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; }

    /// <summary>
    /// Gets when this delegation was revoked (if revoked).
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets who revoked this delegation (if revoked).
    /// </summary>
    public UserId? RevokedBy { get; private set; }

    /// <summary>
    /// Gets the revocation reason.
    /// </summary>
    public string? RevocationReason { get; private set; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets internal remarks.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Determines whether this delegation is currently effective.
    /// </summary>
    public bool IsEffective(DateTime utcNow)
    {
        if (Status == DelegatedAuthorityStatus.Revoked)
            return false;

        if (Status == DelegatedAuthorityStatus.Expired)
            return false;

        if (utcNow < EffectiveFromUtc)
            return false;

        if (EffectiveToUtc.HasValue && utcNow > EffectiveToUtc.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Revokes this delegation.
    /// </summary>
    public void Revoke(UserId revokedBy, string? reason = null)
    {
        ArgumentNullException.ThrowIfNull(revokedBy);

        if (Status == DelegatedAuthorityStatus.Revoked)
        {
            throw new InvalidOperationException(
                "Delegation is already revoked.");
        }

        Status = DelegatedAuthorityStatus.Revoked;
        RevokedAtUtc = DateTime.UtcNow;
        RevokedBy = revokedBy;
        RevocationReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
    }

    /// <summary>
    /// Changes the description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    /// <summary>
    /// Changes the remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }
}
