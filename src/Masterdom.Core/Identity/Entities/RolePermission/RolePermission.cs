using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Primitives;
using Masterdom.Core.Identity.Entities.User;

namespace Masterdom.Core.Identity.Entities.RolePermission;

/// <summary>
/// Represents the assignment of a permission to a role.
/// </summary>
public sealed class RolePermission : AggregateRoot<RolePermissionId>
{
    private RolePermission(
        RolePermissionId id,
        RoleId roleId,
        PermissionId permissionId,
        DateTime assignedAtUtc,
        UserId? assignedBy,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc,
        string? assignmentReason)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(roleId);
        ArgumentNullException.ThrowIfNull(permissionId);

        if (effectiveToUtc.HasValue &&
            effectiveToUtc.Value < effectiveFromUtc)
        {
            throw new InvalidOperationException(
                "EffectiveToUtc cannot be earlier than EffectiveFromUtc.");
        }

        RoleId = roleId;

        PermissionId = permissionId;

        AssignedAtUtc = assignedAtUtc;

        AssignedBy = assignedBy;

        EffectiveFromUtc = effectiveFromUtc;

        EffectiveToUtc = effectiveToUtc;

        AssignmentReason = string.IsNullOrWhiteSpace(assignmentReason)
            ? null
            : assignmentReason.Trim();

        Status = RolePermissionStatus.Active;

        Description = null;

        Remarks = null;

        Other = null;

        DisplayOrder = 0;

        IsHidden = false;
    }

    /// <summary>
    /// Creates a new role-permission assignment.
    /// </summary>
    public static RolePermission Create(
        RoleId roleId,
        PermissionId permissionId,
        UserId? assignedBy = null,
        DateTime? effectiveFromUtc = null,
        DateTime? effectiveToUtc = null,
        string? reason = null)
    {
        var assignedAtUtc = DateTime.UtcNow;
        var effectiveFrom = effectiveFromUtc ?? assignedAtUtc;

        return new RolePermission(
            RolePermissionId.New(),
            roleId,
            permissionId,
            assignedAtUtc,
            assignedBy,
            effectiveFrom,
            effectiveToUtc,
            reason);
    }

    public RoleId RoleId { get; }

    public PermissionId PermissionId { get; }

    public DateTime AssignedAtUtc { get; }

    public UserId? AssignedBy { get; }

    public DateTime EffectiveFromUtc { get; private set; }

    public DateTime? EffectiveToUtc { get; private set; }

    public string? AssignmentReason { get; private set; }

    public RolePermissionStatus Status { get; private set; }

    public DateTime? RevokedAtUtc { get; private set; }

    public UserId? RevokedBy { get; private set; }

    public string? RevocationReason { get; private set; }

    public string? Description { get; private set; }

    public string? Remarks { get; private set; }

    public string? Other { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsHidden { get; private set; }

    public bool IsEffective(DateTime utcNow)
    {
        if (Status != RolePermissionStatus.Active)
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
        if (Status == RolePermissionStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived role permission cannot be activated.");
        }

        Status = RolePermissionStatus.Active;
    }

    /// <summary>
    /// Deactivates the assignment.
    /// </summary>
    public void Deactivate()
    {
        if (Status == RolePermissionStatus.Archived)
        {
            throw new InvalidOperationException(
                "An archived role permission cannot be deactivated.");
        }

        Status = RolePermissionStatus.Inactive;
    }

    /// <summary>
    /// Archives the assignment.
    /// </summary>
    public void Archive()
    {
        Status = RolePermissionStatus.Archived;
    }

    /// <summary>
    /// Revokes the assignment.
    /// </summary>
    public void Revoke(
        DateTime revokedAtUtc,
        UserId? revokedBy,
        string? reason = null)
    {
        if (Status != RolePermissionStatus.Active)
        {
            throw new InvalidOperationException(
                "Only an active role permission can be revoked.");
        }

        Status = RolePermissionStatus.Inactive;

        RevokedAtUtc = revokedAtUtc;

        RevokedBy = revokedBy;

        RevocationReason = string.IsNullOrWhiteSpace(reason)
            ? null
            : reason.Trim();
    }

    /// <summary>
    /// Extends or shortens the effective period.
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
    /// Changes remarks.
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
