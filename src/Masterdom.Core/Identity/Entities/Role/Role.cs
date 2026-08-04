using System.Collections.ObjectModel;
using Masterdom.Core.Identity.Entities.Permission;
using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Role;

/// <summary>
/// Represents an application role.
/// </summary>
public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<PermissionId> _permissionIds = [];

    private Role(
        RoleId id,
        RoleCode code,
        RoleName name)
        : base(id)
    {
        Code = code;
        Name = name;

        Status = RoleStatus.Active;

        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    public static Role Create(
        RoleCode code,
        RoleName name)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        return new Role(
            RoleId.New(),
            code,
            name);
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public RoleCode Code { get; }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public RoleName Name { get; private set; }

    /// <summary>
    /// Gets the lifecycle status.
    /// </summary>
    public RoleStatus Status { get; private set; }

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
    /// Gets the effective start date.
    /// </summary>
    public DateTime? EffectiveFromUtc { get; private set; }

    /// <summary>
    /// Gets the effective end date.
    /// </summary>
    public DateTime? EffectiveToUtc { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets whether the role is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Gets the permissions assigned to this role.
    /// </summary>
    public IReadOnlyCollection<PermissionId> PermissionIds =>
        new ReadOnlyCollection<PermissionId>(_permissionIds);

    public void Rename(RoleName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Name == name)
            return;

        Name = name;
    }

    public void AddPermission(PermissionId permissionId)
    {
        ArgumentNullException.ThrowIfNull(permissionId);

        if (_permissionIds.Contains(permissionId))
            return;

        _permissionIds.Add(permissionId);
    }

    public void RemovePermission(PermissionId permissionId)
    {
        ArgumentNullException.ThrowIfNull(permissionId);

        _permissionIds.Remove(permissionId);
    }

    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }

    public void ChangeOther(string? other)
    {
        Other = string.IsNullOrWhiteSpace(other)
            ? null
            : other.Trim();
    }

    public void SetEffectivePeriod(DateTime? fromUtc, DateTime? toUtc)
    {
        if (fromUtc.HasValue &&
            toUtc.HasValue &&
            fromUtc > toUtc)
        {
            throw new InvalidOperationException(
                "EffectiveFromUtc cannot be after EffectiveToUtc.");
        }

        EffectiveFromUtc = fromUtc;
        EffectiveToUtc = toUtc;
    }

    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        DisplayOrder = displayOrder;
    }

    public void Hide() => IsHidden = true;

    public void Show() => IsHidden = false;

    public void Activate() => Status = RoleStatus.Active;

    public void Deactivate()
    {
        if (Status == RoleStatus.Archived)
            throw new InvalidOperationException(
                "An archived role cannot be deactivated.");

        Status = RoleStatus.Inactive;
    }

    public void Archive() => Status = RoleStatus.Archived;
}
