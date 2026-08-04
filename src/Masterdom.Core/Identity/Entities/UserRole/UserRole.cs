using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.UserRole;

/// <summary>
/// Represents the assignment of a role to a user.
/// A user may have multiple roles over time.
/// </summary>
public sealed class UserRole : AggregateRoot<UserRoleId>
{
    private UserRole(
        UserRoleId id,
        UserId userId,
        RoleId roleId,
        DateTime assignedAtUtc,
        UserId? assignedBy,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        bool isPrimaryRole,
        string? assignmentReason)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(roleId);

        if (effectiveToUtc.HasValue &&
            effectiveToUtc.Value < effectiveFromUtc)
        {
            throw new InvalidOperationException(
                "EffectiveToUtc cannot be earlier than EffectiveFromUtc.");
        }

        UserId = userId;

        RoleId = roleId;

        AssignedAtUtc = assignedAtUtc;

        AssignedBy = assignedBy;

        EffectiveFromUtc = effectiveFromUtc;

        EffectiveToUtc = effectiveToUtc;

        IsPrimaryRole = isPrimaryRole;

        AssignmentReason = string.IsNullOrWhiteSpace(assignmentReason)
            ? null
            : assignmentReason.Trim();

        Status = UserRoleStatus.Active;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new user-role assignment.
    /// </summary>
    public static UserRole Create(
        UserId userId,
        RoleId roleId,
        UserId? assignedBy = null,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        bool isPrimaryRole = false,
        string? reason = null)
    {
        var assignedAtUtc = DateTime.UtcNow;
        var effectiveFrom = effectiveFromUtc ?? assignedAtUtc;

        return new UserRole(
            UserRoleId.New(),
            userId,
            roleId,
            assignedAtUtc,
            assignedBy,
            effectiveFrom,
            effectiveToUtc,
            isPrimaryRole,
            reason);
    }

    /// <summary>
    /// Gets the user.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Gets the assigned role.
    /// </summary>
    public RoleId RoleId { get; }

    /// <summary>
    /// Gets when the assignment was created.
    /// </summary>
    public DateTime AssignedAtUtc { get; }

    /// <summary>
    /// Gets who assigned the role.
    /// </summary>
    public UserId? AssignedBy { get; }

    /// <summary>
    /// Gets when the assignment becomes effective.
    /// </summary>
    public DateTime EffectiveFromUtc { get; private set; }

    /// <summary>
    /// Gets when the assignment expires.
    /// </summary>
    public DateTime? EffectiveToUtc { get; private set; }

    /// <summary>
    /// Gets whether this is the user's primary role.
    /// </summary>
    public bool IsPrimaryRole { get; private set; }

    /// <summary>
    /// Gets the assignment reason.
    /// </summary>
    public string? AssignmentReason { get; private set; }

    /// <summary>
    /// Gets the assignment status.
    /// </summary>
    public UserRoleStatus Status { get; private set; }

    /// <summary>
    /// Gets the revocation timestamp.
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets who revoked the assignment.
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
    /// Gets configurable additional information.
    /// </summary>
    public string? Other { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets whether the assignment is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Determines whether the assignment is effective.
    /// </summary>
    public bool IsEffective(DateTime utcNow)
    {
        if (Status != UserRoleStatus.Active)
            return false;

        if (utcNow < EffectiveFromUtc)
            return false;

        if (EffectiveToUtc.HasValue &&
            utcNow > EffectiveToUtc.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Activates the assignment.
    /// </summary>
    public void Activate()
    {
        if (Status == UserRoleStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived role assignment cannot be activated.");
        }

        Status = UserRoleStatus.Active;
    }

    /// <summary>
    /// Deactivates the assignment.
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserRoleStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived role assignment cannot be deactivated.");
        }

        Status = UserRoleStatus.Inactive;
    }

    /// <summary>
    /// Archives the assignment.
    /// </summary>
    public void Archive()
    {
        Status = UserRoleStatus.Archived;
    }

    /// <summary>
    /// Revokes the assignment.
    /// </summary>
    public void Revoke(
        DateTime revokedAtUtc,
        UserId? revokedBy,
        string? reason = null)
    {
        if (Status != UserRoleStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active role assignment can be revoked.");
        }

        Status = UserRoleStatus.Inactive;

        RevokedAtUtc = revokedAtUtc;

        RevokedBy = revokedBy;

        RevocationReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
    }

    /// <summary>
    /// Extends or shortens the assignment expiry.
    /// </summary>
    public void ExtendUntil(DateTime? effectiveToUtc)
    {
        if (effectiveToUtc.HasValue &&
            effectiveToUtc.Value < EffectiveFromUtc)
        {
            throw new InvalidOperationException(
                "EffectiveToUtc cannot be earlier than EffectiveFromUtc.");
        }

        EffectiveToUtc = effectiveToUtc;
    }

    /// <summary>
    /// Sets the effective period.
    /// </summary>
    public void SetEffectivePeriod(
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc)
    {
        if (effectiveToUtc.HasValue &&
            effectiveToUtc.Value < effectiveFromUtc)
        {
            throw new InvalidOperationException(
                "EffectiveToUtc cannot be earlier than EffectiveFromUtc.");
        }

        EffectiveFromUtc = effectiveFromUtc;

        EffectiveToUtc = effectiveToUtc;
    }

    /// <summary>
    /// Marks this as the user's primary role.
    /// </summary>
    public void MakePrimary()
    {
        IsPrimaryRole = true;
    }

    /// <summary>
    /// Removes the primary role designation.
    /// </summary>
    public void RemovePrimary()
    {
        IsPrimaryRole = false;
    }

    /// <summary>
    /// Changes the assignment reason.
    /// </summary>
    public void ChangeAssignmentReason(string? reason)
    {
        AssignmentReason = string.IsNullOrWhiteSpace(reason)
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
    /// Changes internal remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }

    /// <summary>
    /// Changes the configurable other field.
    /// </summary>
    public void ChangeOther(string? other)
    {
        Other = string.IsNullOrWhiteSpace(other)
            ? null
            : other.Trim();
    }

    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(displayOrder));
        }

        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Hides the assignment.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Shows the assignment.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
}
