using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;


/// <summary>
/// Represents a unit within a property.
/// </summary>
public sealed class Unit : Entity<UnitId>
{
    internal Unit(
        UnitId id,
        UnitCode code,
        UnitName name,
        UnitType type,
        OccupancyStatus status,
        Capacity capacity)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(capacity);

        Code = code;
        Name = name;
        Type = type;
        Status = status;
        Capacity = capacity;

        Description = null;
        Remarks = null;
        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Gets the unit code.
    /// </summary>
    public UnitCode Code { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public UnitName Name { get; private set; }

    /// <summary>
    /// Gets the unit type.
    /// </summary>
    public UnitType Type { get; private set; }

    /// <summary>
    /// Gets the maximum occupancy capacity.
    /// </summary>
    public Capacity Capacity { get; private set; }

    /// <summary>
    /// Gets the occupancy status.
    /// </summary>
    public OccupancyStatus Status { get; private set; }

    /// <summary>
    /// Gets the parent unit identifier if this unit is nested.
    /// </summary>
    public UnitId? ParentUnitId { get; private set; }

    /// <summary>
    /// Renames the unit.
    /// </summary>
    public void Rename(UnitName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Name == name)
            return;

        Name = name;
    }

    /// <summary>
    /// Updates the unit description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    /// <summary>
    /// Updates internal remarks.
    /// </summary>
    public void ChangeRemarks(string? remarks)
    {
        Remarks = string.IsNullOrWhiteSpace(remarks)
            ? null
            : remarks.Trim();
    }

    /// <summary>
    /// Sets display order within the parent property.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        DisplayOrder = displayOrder;
    }

    /// <summary>
    /// Sets the unit capacity.
    /// </summary>
    public void SetCapacity(Capacity capacity)
    {
        ArgumentNullException.ThrowIfNull(capacity);
        Capacity = capacity;
    }

    /// <summary>
    /// Assigns a parent unit relationship.
    /// </summary>
    public void AssignParentUnit(UnitId? parentUnitId)
    {
        if (parentUnitId == Id)
        {
            throw new InvalidOperationException("A unit cannot reference itself as parent.");
        }

        ParentUnitId = parentUnitId;
    }

    /// <summary>
    /// Hides the unit from operational views.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }

    /// <summary>
    /// Makes the unit visible.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }

    /// <summary>
    /// Updates the occupancy status.
    /// </summary>
    public void SetOccupancy(OccupancyStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        if (Status == status)
            return;

        Status = status;
    }
    /// <summary>
    /// Gets the optional unit description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets optional internal remarks.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Gets whether the unit is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    internal PropertyId PropertyId { get; private set; } = default!;

    internal void AttachToProperty(PropertyId propertyId)
    {
        if (PropertyId != default && PropertyId != propertyId)
        {
            throw new InvalidOperationException("A unit cannot be reassigned to a different property.");
        }

        PropertyId = propertyId;
    }
}
