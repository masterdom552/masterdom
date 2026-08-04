using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Modules.Properties.Domain.Entities.Property.Events;
using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Properties.Domain.Entities.Property;

/// <summary>
/// Represents a managed property within the Masterdom platform.
/// </summary>
public sealed class Property : AggregateRoot<PropertyId>, IHasDomainEvents
{
    private readonly List<Unit> _units = [];
    private readonly List<PropertyMetadata> _metadata = [];
    private readonly List<PropertyRelationship> _relationships = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Property(
    PropertyId id,
    PropertyCode code,
    PropertyName name,
    PropertyType type,
    PropertyStatus status)
        : base(id)
    {
        Code = code;
        Name = name;
        Type = type;
        Status = status;

        Description = null;
        Remarks = null;
        OwnerId = null;
        ParentPropertyId = null;
        Address = null;
        Settings = PropertySettings.Default;

        EffectiveFromUtc = null;
        EffectiveToUtc = null;
        DisplayOrder = 0;
        IsHidden = false;
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    public static Property Create(
    PropertyCode code,
    PropertyName name,
    PropertyType type)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(name);

        var property = new Property(
            PropertyId.New(),
            code,
            name,
            type,
            PropertyStatus.Active);

        property.Raise(new PropertyCreatedDomainEvent(property.Id, property.Code));

        return property;
    }

    /// <summary>
    /// Gets the business code.
    /// </summary>
    public PropertyCode Code { get; }

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public PropertyName Name { get; private set; }

    /// <summary>
    /// Gets the property type.
    /// </summary>
    public PropertyType Type { get; private set; }

    /// <summary>
    /// Gets the current status.
    /// </summary>
    public PropertyStatus Status { get; private set; }

    /// <summary>
    /// Gets the optional property description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets optional internal remarks.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Gets the owner identifier.
    /// </summary>
    public Guid? OwnerId { get; private set; }

    /// <summary>
    /// Gets the parent property identifier.
    /// </summary>
    public PropertyId? ParentPropertyId { get; private set; }

    /// <summary>
    /// Gets the postal address.
    /// </summary>
    public PropertyAddress? Address { get; private set; }

    /// <summary>
    /// Gets operational settings.
    /// </summary>
    public PropertySettings Settings { get; private set; }

    /// <summary>
    /// Gets the date from which the property is effective.
    /// </summary>
    public DateTime? EffectiveFromUtc { get; private set; }

    /// <summary>
    /// Gets the date until which the property is effective.
    /// </summary>
    public DateTime? EffectiveToUtc { get; private set; }

    /// <summary>
    /// Gets the display order.
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// Gets whether the property is hidden.
    /// </summary>
    public bool IsHidden { get; private set; }
    /// <summary>
    /// Gets the units belonging to this property.
    /// </summary>
    public IReadOnlyCollection<Unit> Units => _units;

    /// <summary>
    /// Gets property metadata records.
    /// </summary>
    public IReadOnlyCollection<PropertyMetadata> Metadata => _metadata;

    /// <summary>
    /// Gets property relationships.
    /// </summary>
    public IReadOnlyCollection<PropertyRelationship> Relationships => _relationships;

    /// <inheritdoc />
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Renames the property.
    /// </summary>
    public void Rename(PropertyName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (Name == name)
            return;

        Name = name;
        Raise(new PropertyRenamedDomainEvent(Id, name));
    }
    /// <summary>
    /// Updates the property description.
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
    /// Changes the owner.
    /// </summary>
    public void ChangeOwner(Guid? ownerId)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// Changes the property address.
    /// </summary>
    public void ChangeAddress(PropertyAddress? address)
    {
        Address = address;
    }

    /// <summary>
    /// Applies operational property settings.
    /// </summary>
    public void ConfigureSettings(PropertySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Settings = settings;
    }
    /// <summary>
    /// Changes the parent property.
    /// </summary>
    public void ChangeParentProperty(PropertyId? parentPropertyId)
    {
        if (parentPropertyId == Id)
            throw new InvalidOperationException(
                "A property cannot reference itself as its parent.");

        if (ParentPropertyId == parentPropertyId)
            return;

        ParentPropertyId = parentPropertyId;
    }
    /// <summary>
    /// Sets the effective period.
    /// </summary>
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
    /// <summary>
    /// Sets the display order.
    /// </summary>
    public void SetDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(displayOrder));

        DisplayOrder = displayOrder;
    }
    /// <summary>
    /// Hides the property.
    /// </summary>
    public void Hide()
    {
        IsHidden = true;
    }
    /// <summary>
    /// Makes the property visible.
    /// </summary>
    public void Show()
    {
        IsHidden = false;
    }
    /// <summary>
    /// Changes the property type.
    /// </summary>
    public void ChangeType(PropertyType type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (Type == type)
            return;

        if (_units.Any())
            throw new InvalidOperationException(
                "Property type cannot be changed after units have been created.");

        Type = type;
    }
    /// <summary>
    /// Activates the property.
    /// </summary>
    public void Activate()
    {
        if (Status == PropertyStatus.Active)
            return;

        Status = PropertyStatus.Active;
        Raise(new PropertyStatusChangedDomainEvent(Id, Status));
    }

    /// <summary>
    /// Deactivates the property.
    /// </summary>
    public void Deactivate()
    {
        if (Status == PropertyStatus.Inactive)
            return;

        if (Status == PropertyStatus.Archived)
            throw new InvalidOperationException(
                "An archived property cannot be deactivated.");

        Status = PropertyStatus.Inactive;
        Raise(new PropertyStatusChangedDomainEvent(Id, Status));
    }
    /// <summary>
    /// Archives the property.
    /// </summary>
    public void Archive()
    {
        if (Status == PropertyStatus.Archived)
            return;

        if (_units.Any())
            throw new InvalidOperationException(
                "A property containing units cannot be archived.");

        Status = PropertyStatus.Archived;
        Raise(new PropertyStatusChangedDomainEvent(Id, Status));
    }

    /// <summary>
    /// Adds or replaces a metadata entry.
    /// </summary>
    public void UpsertMetadata(PropertyMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var existing = _metadata.FindIndex(x => x.Key == metadata.Key);
        if (existing >= 0)
        {
            _metadata[existing] = metadata;
            return;
        }

        _metadata.Add(metadata);
    }

    /// <summary>
    /// Removes metadata by key.
    /// </summary>
    public bool RemoveMetadata(string key)
    {
        var normalized = PropertyMetadata.NormalizeKey(key);
        var existing = _metadata.FirstOrDefault(x => x.Key == normalized);

        if (existing is null)
        {
            return false;
        }

        _metadata.Remove(existing);
        return true;
    }

    /// <summary>
    /// Adds a relationship to another property.
    /// </summary>
    public void AddRelationship(PropertyRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(relationship);

        if (relationship.TargetPropertyId == Id)
        {
            throw new InvalidOperationException("A property cannot reference itself in relationships.");
        }

        if (_relationships.Contains(relationship))
        {
            return;
        }

        _relationships.Add(relationship);
    }

    /// <summary>
    /// Removes a relationship from this property.
    /// </summary>
    public bool RemoveRelationship(PropertyId targetPropertyId, PropertyRelationshipType type)
    {
        var relationship = _relationships.FirstOrDefault(x =>
            x.TargetPropertyId == targetPropertyId && x.Type == type);

        if (relationship is null)
        {
            return false;
        }

        _relationships.Remove(relationship);
        return true;
    }
    /// <summary>
    /// Adds an existing unit.
    /// </summary>
    public void AddUnit(Unit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        if (Status == PropertyStatus.Archived)
            throw new InvalidOperationException(
                "Units cannot be added to an archived property.");

        if (_units.Any(x => x.Id == unit.Id))
            throw new InvalidOperationException(
                $"Unit '{unit.Id}' already exists.");

        if (_units.Any(x => x.Code == unit.Code))
            throw new InvalidOperationException(
                $"Unit code '{unit.Code}' already exists.");

        unit.AttachToProperty(Id);

        _units.Add(unit);

        Raise(new UnitCreatedDomainEvent(Id, unit.Id, unit.Code));
    }
    /// <summary>
    /// Creates and adds a unit to the property.
    /// </summary>
    public Unit CreateUnit(UnitCode code, string name, UnitType type, Capacity? capacity = null)
    {
        if (Status == PropertyStatus.Archived)
            throw new InvalidOperationException(
                "Units cannot be created for an archived property.");
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(type);

        if (_units.Any(x => x.Code == code))
            throw new InvalidOperationException($"A unit with code '{code}' already exists.");

        var unit = new Unit(
            UnitId.New(),
            code,
            new UnitName(name),
            type,
            OccupancyStatus.Vacant,
            capacity ?? new Capacity(1));

        unit.AttachToProperty(Id);

        _units.Add(unit);
        Raise(new UnitCreatedDomainEvent(Id, unit.Id, unit.Code));

        return unit;
    }

    /// <summary>
    /// Removes a unit from the property.
    /// </summary>
    public bool RemoveUnit(UnitId unitId)
    {
        var unit = _units.FirstOrDefault(x => x.Id == unitId);

        if (unit is null)
        {
            return false;
        }

        _units.Remove(unit);

        Raise(new UnitRemovedDomainEvent(Id, unit.Id));

        return true;
    }

    /// <inheritdoc />
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
