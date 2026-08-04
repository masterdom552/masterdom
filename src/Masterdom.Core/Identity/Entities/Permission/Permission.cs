using Masterdom.Core.Primitives;

namespace Masterdom.Core.Identity.Entities.Permission;

/// <summary>
/// Represents an application permission.
/// </summary>
public sealed class Permission : AggregateRoot<PermissionId>
{
    private Permission(
        PermissionId id,
        PermissionCode code,
        PermissionName name)
        : base(id)
    {
        Code = code;
        Name = name;

        Status = PermissionStatus.Active;

        Description = null;
        Remarks = null;
        Other = null;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;

        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    public static Permission Create(
        PermissionCode code,
        PermissionName name)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        return new Permission(
            PermissionId.New(),
            code,
            name);
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public PermissionCode Code { get; }

    /// <summary>
    /// Gets the permission name.
    /// </summary>
    public PermissionName Name { get; private set; }

    /// <summary>
    /// Gets the lifecycle status.
    /// </summary>
    public PermissionStatus Status { get; private set; }

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
    /// Gets whether the permission is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    public void Rename(PermissionName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Name == name)
            return;

        Name = name;
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

    public void Activate() => Status = PermissionStatus.Active;

    public void Deactivate()
    {
        if (Status == PermissionStatus.Archived)
            throw new InvalidOperationException(
                "An archived permission cannot be deactivated.");

        Status = PermissionStatus.Inactive;
    }

    public void Archive() => Status = PermissionStatus.Archived;
}
