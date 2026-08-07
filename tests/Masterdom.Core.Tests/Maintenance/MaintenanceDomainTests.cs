using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.Events;

namespace Masterdom.Core.Tests.Maintenance;

public sealed class MaintenanceDomainTests
{
    [Fact]
    public void Create_ShouldCreateOpenTicketAndRaiseCreatedEvent()
    {
        var createdAtUtc = DateTime.UtcNow;

        var maintenanceTicket = MaintenanceTicket.Create(
            MaintenanceTicketId.New(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Water leak",
            "Leak reported near kitchen sink.",
            createdAtUtc);

        Assert.Equal(MaintenanceTicketStatus.Open, maintenanceTicket.Status);
        Assert.Equal("Water leak", maintenanceTicket.Title);
        Assert.Contains(maintenanceTicket.DomainEvents, x => x is MaintenanceTicketCreatedDomainEvent);
    }

    [Fact]
    public void Assign_ShouldSetAssigneeAndRaiseAssignedEvent()
    {
        var maintenanceTicket = MaintenanceTicket.Create(
            MaintenanceTicketId.New(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Power issue",
            "Power outage in hallway.",
            DateTime.UtcNow);

        var assignedToPersonId = Guid.NewGuid();
        var assignedAtUtc = DateTime.UtcNow;

        maintenanceTicket.Assign(assignedToPersonId, assignedAtUtc);

        Assert.Equal(assignedToPersonId, maintenanceTicket.AssignedToPersonId);
        Assert.Equal(assignedAtUtc, maintenanceTicket.AssignedAtUtc);
        Assert.Contains(maintenanceTicket.DomainEvents, x => x is MaintenanceTicketAssignedDomainEvent);
    }

    [Fact]
    public void Close_ShouldSetClosedStatusAndRaiseClosedEvent()
    {
        var maintenanceTicket = MaintenanceTicket.Create(
            MaintenanceTicketId.New(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Window issue",
            "Window does not lock.",
            DateTime.UtcNow.AddMinutes(-5));

        var closedAtUtc = DateTime.UtcNow;
        maintenanceTicket.Close(closedAtUtc);

        Assert.Equal(MaintenanceTicketStatus.Closed, maintenanceTicket.Status);
        Assert.Contains(maintenanceTicket.DomainEvents, x => x is MaintenanceTicketClosedDomainEvent);
    }
}
