using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Identifiers;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;

/// <summary>
/// Represents a tenancy aggregate responsible for occupancy lifecycle invariants.
/// </summary>
public sealed class Tenancy : AggregateRoot<TenancyId>, IHasDomainEvents
{
    private readonly List<OccupantReference> _occupants = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    private Tenancy(
        TenancyId id,
        TenancyNumber number,
        PropertyReference property,
        UnitReference unit,
        MoveInDate moveInDate,
        Notes? notes)
        : base(id)
    {
        Number = number;
        Property = property;
        Unit = unit;
        MoveInDate = moveInDate;
        Notes = notes;

        Status = TenancyStatus.Active;
        OccupancyStatus = moveInDate.Value > DateOnly.FromDateTime(DateTime.UtcNow)
            ? OccupancyStatus.Scheduled
            : OccupancyStatus.Occupied;
    }

    public TenancyNumber Number { get; }

    public PropertyReference Property { get; }

    public UnitReference Unit { get; }

    public MoveInDate MoveInDate { get; private set; }

    public MoveOutDate? MoveOutDate { get; private set; }

    public TenancyStatus Status { get; private set; }

    public OccupancyStatus OccupancyStatus { get; private set; }

    public EffectiveDate? ClosedOn { get; private set; }

    public TerminationReason? TerminationReason { get; private set; }

    public Notes? Notes { get; private set; }

    public IReadOnlyCollection<OccupantReference> Occupants => _occupants.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Tenancy Create(
        TenancyNumber number,
        PropertyReference property,
        UnitReference unit,
        MoveInDate moveInDate,
        OccupantReference primaryOccupant,
        Notes? notes)
    {
        ArgumentNullException.ThrowIfNull(number);
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(unit);
        ArgumentNullException.ThrowIfNull(moveInDate);
        ArgumentNullException.ThrowIfNull(primaryOccupant);

        if (!primaryOccupant.IsPrimary)
        {
            throw new InvalidOperationException("A tenancy must be created with a primary occupant.");
        }

        var tenancy = new Tenancy(
            TenancyId.New(),
            number,
            property,
            unit,
            moveInDate,
            notes);

        tenancy._occupants.Add(primaryOccupant);

        tenancy.Raise(new TenancyCreatedDomainEvent(tenancy.Id, tenancy.Number, DateTime.UtcNow));
        tenancy.Raise(new OccupantAddedDomainEvent(tenancy.Id, primaryOccupant.PersonId, true, DateTime.UtcNow));

        return tenancy;
    }

    public void AddOccupant(PersonId personId, bool isPrimary)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(personId);

        if (_occupants.Any(x => x.PersonId == personId))
        {
            throw new InvalidOperationException("The occupant is already assigned to this tenancy.");
        }

        if (isPrimary)
        {
            for (var i = 0; i < _occupants.Count; i++)
            {
                _occupants[i] = OccupantReference.Create(_occupants[i].PersonId, false);
            }
        }

        var reference = OccupantReference.Create(personId, isPrimary);
        _occupants.Add(reference);

        EnsurePrimaryOccupantExists();

        Raise(new OccupantAddedDomainEvent(Id, personId, isPrimary, DateTime.UtcNow));
    }

    public void RemoveOccupant(PersonId personId)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(personId);

        var occupant = _occupants.SingleOrDefault(x => x.PersonId == personId);
        if (occupant is null)
        {
            throw new InvalidOperationException("The occupant is not assigned to this tenancy.");
        }

        if (occupant.IsPrimary)
        {
            var hasAnotherPrimary = _occupants.Any(x => x.PersonId != personId && x.IsPrimary);
            if (!hasAnotherPrimary)
            {
                throw new InvalidOperationException(
                    "Primary occupant cannot be removed before assigning another primary occupant.");
            }
        }

        _occupants.Remove(occupant);
        EnsurePrimaryOccupantExists();

        Raise(new OccupantRemovedDomainEvent(Id, personId, DateTime.UtcNow));
    }

    public void RecordMoveIn(MoveInDate moveInDate)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(moveInDate);

        if (MoveOutDate is not null && moveInDate.Value >= MoveOutDate.Value)
        {
            throw new InvalidOperationException("Move-in date must be earlier than move-out date.");
        }

        MoveInDate = moveInDate;
        OccupancyStatus = OccupancyStatus.Occupied;

        Raise(new MoveInRecordedDomainEvent(Id, moveInDate, DateTime.UtcNow));
    }

    public void RecordMoveOut(MoveOutDate moveOutDate)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(moveOutDate);

        if (moveOutDate.Value <= MoveInDate.Value)
        {
            throw new InvalidOperationException("Move-out date must be after move-in date.");
        }

        MoveOutDate = moveOutDate;
        OccupancyStatus = OccupancyStatus.Vacated;

        Raise(new MoveOutRecordedDomainEvent(Id, moveOutDate, DateTime.UtcNow));
    }

    public void Close(EffectiveDate closedOn, TerminationReason reason)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(closedOn);
        ArgumentNullException.ThrowIfNull(reason);

        if (closedOn.Value < MoveInDate.Value)
        {
            throw new InvalidOperationException("Tenancy cannot be closed before move-in date.");
        }

        ClosedOn = closedOn;
        TerminationReason = reason;
        Status = TenancyStatus.Closed;

        Raise(new TenancyClosedDomainEvent(Id, closedOn, reason, DateTime.UtcNow));
    }

    public void Archive()
    {
        if (Status == TenancyStatus.Archived)
        {
            return;
        }

        if (Status == TenancyStatus.Active)
        {
            throw new InvalidOperationException("Active tenancy cannot be archived.");
        }

        Status = TenancyStatus.Archived;

        Raise(new TenancyArchivedDomainEvent(Id, DateTime.UtcNow));
    }

    public void UpdateNotes(Notes? notes)
    {
        EnsureMutable();
        Notes = notes;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void EnsurePrimaryOccupantExists()
    {
        if (_occupants.Count == 0)
        {
            throw new InvalidOperationException("A tenancy requires at least one occupant.");
        }

        if (_occupants.All(x => !x.IsPrimary))
        {
            throw new InvalidOperationException("A tenancy requires a primary occupant.");
        }
    }

    private void EnsureMutable()
    {
        if (Status == TenancyStatus.Closed)
        {
            throw new InvalidOperationException("Closed tenancy cannot be modified.");
        }

        if (Status == TenancyStatus.Archived)
        {
            throw new InvalidOperationException("Archived tenancy cannot be modified.");
        }
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
