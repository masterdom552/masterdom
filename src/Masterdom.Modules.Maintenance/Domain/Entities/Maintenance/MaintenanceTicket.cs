using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.Events;

namespace Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;

public sealed class MaintenanceTicket : AggregateRoot<MaintenanceTicketId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    private MaintenanceTicket(
        MaintenanceTicketId id,
        Guid propertyId,
        Guid unitId,
        string title,
        string description,
        DateTime createdAtUtc)
        : base(id)
    {
        PropertyId = propertyId;
        UnitId = unitId;
        Title = title;
        Description = description;
        Status = MaintenanceTicketStatus.Open;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid PropertyId { get; private set; }

    public Guid UnitId { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public MaintenanceTicketStatus Status { get; private set; }

    public Guid? AssignedToPersonId { get; private set; }

    public DateTime? AssignedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static MaintenanceTicket Create(
        MaintenanceTicketId id,
        Guid propertyId,
        Guid unitId,
        string title,
        string description,
        DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (propertyId == Guid.Empty)
        {
            throw new ArgumentException("PropertyId cannot be empty.", nameof(propertyId));
        }

        if (unitId == Guid.Empty)
        {
            throw new ArgumentException("UnitId cannot be empty.", nameof(unitId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (title.Length > 200)
        {
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));
        }

        if (description.Length > 2000)
        {
            throw new ArgumentException("Description cannot exceed 2000 characters.", nameof(description));
        }

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("CreatedAtUtc must be in UTC.");
        }

        var ticket = new MaintenanceTicket(
            id,
            propertyId,
            unitId,
            title.Trim(),
            description.Trim(),
            createdAtUtc);

        ticket.Raise(new MaintenanceTicketCreatedDomainEvent(ticket.Id, propertyId, unitId, createdAtUtc));

        return ticket;
    }

    public void Assign(Guid assignedToPersonId, DateTime assignedAtUtc)
    {
        if (assignedToPersonId == Guid.Empty)
        {
            throw new ArgumentException("AssignedToPersonId cannot be empty.", nameof(assignedToPersonId));
        }

        if (assignedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("AssignedAtUtc must be in UTC.");
        }

        if (Status == MaintenanceTicketStatus.Closed)
        {
            throw new InvalidOperationException("Closed maintenance tickets cannot be assigned.");
        }

        AssignedToPersonId = assignedToPersonId;
        AssignedAtUtc = assignedAtUtc;

        Raise(new MaintenanceTicketAssignedDomainEvent(
            Id,
            PropertyId,
            assignedToPersonId,
            assignedAtUtc));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
