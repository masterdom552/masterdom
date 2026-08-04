using Masterdom.Core.Identifiers;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Core.Tests.Tenancy;

public sealed class TenancyDomainTests
{
    [Fact]
    public void Create_ShouldInitializeTenancyAndRaiseEvents()
    {
        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0001"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(PersonId.New(), true),
            Notes.Create("Initial tenancy"));

        Assert.NotNull(tenancy);
        Assert.Equal(TenancyStatus.Active, tenancy.Status);
        Assert.Single(tenancy.Occupants);
        Assert.Contains(tenancy.Occupants, x => x.IsPrimary);
        Assert.Contains(tenancy.DomainEvents, x => x is TenancyCreatedDomainEvent);
        Assert.Contains(tenancy.DomainEvents, x => x is OccupantAddedDomainEvent);
    }

    [Fact]
    public void Create_ShouldThrow_WhenPrimaryOccupantFlagIsFalse()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            TenancyAggregate.Create(
                TenancyNumber.Create("TEN-0002"),
                PropertyReference.Create(Guid.NewGuid()),
                UnitReference.Create(Guid.NewGuid()),
                MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
                OccupantReference.Create(PersonId.New(), false),
                notes: null));

        Assert.Equal("A tenancy must be created with a primary occupant.", exception.Message);
    }

    [Fact]
    public void RecordMoveOut_ShouldThrow_WhenMoveOutIsNotAfterMoveIn()
    {
        var moveIn = DateOnly.FromDateTime(DateTime.UtcNow);

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0003"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(moveIn),
            OccupantReference.Create(PersonId.New(), true),
            notes: null);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            tenancy.RecordMoveOut(MoveOutDate.Create(moveIn)));

        Assert.Equal("Move-out date must be after move-in date.", exception.Message);
    }

    [Fact]
    public void RemoveOccupant_ShouldThrow_WhenRemovingOnlyPrimaryOccupant()
    {
        var primaryPersonId = PersonId.New();

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0004"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(primaryPersonId, true),
            notes: null);

        var exception = Assert.Throws<InvalidOperationException>(() => tenancy.RemoveOccupant(primaryPersonId));

        Assert.Equal("Primary occupant cannot be removed before assigning another primary occupant.", exception.Message);
    }

    [Fact]
    public void Archive_ShouldMakeAggregateImmutable()
    {
        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0005"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(PersonId.New(), true),
            notes: null);

        tenancy.Close(
            EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            TerminationReason.Create("Lease completed"));

        tenancy.Archive();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            tenancy.UpdateNotes(Notes.Create("Cannot update archived tenancy")));

        Assert.Equal("Archived tenancy cannot be modified.", exception.Message);
        Assert.Contains(tenancy.DomainEvents, x => x is TenancyArchivedDomainEvent);
    }

    [Fact]
    public void Create_ShouldAllowFutureTenancyAsScheduled()
    {
        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0006"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))),
            OccupantReference.Create(PersonId.New(), true),
            notes: null);

        Assert.Equal(OccupancyStatus.Scheduled, tenancy.OccupancyStatus);
    }

    [Fact]
    public void RecordMoveIn_ShouldUpdateMoveInDateAndOccupancy()
    {
        var originalMoveIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var actualMoveIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0007"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(originalMoveIn),
            OccupantReference.Create(PersonId.New(), true),
            notes: null);

        tenancy.RecordMoveIn(MoveInDate.Create(actualMoveIn));

        Assert.Equal(actualMoveIn, tenancy.MoveInDate.Value);
        Assert.Equal(OccupancyStatus.Occupied, tenancy.OccupancyStatus);
        Assert.Contains(tenancy.DomainEvents, x => x is MoveInRecordedDomainEvent recorded && recorded.MoveInDate.Value == actualMoveIn);
    }

    [Fact]
    public void ClosedTenancy_ShouldBeImmutableUntilArchived()
    {
        var tenancy = TenancyAggregate.Create(
            TenancyNumber.Create("TEN-0008"),
            PropertyReference.Create(Guid.NewGuid()),
            UnitReference.Create(Guid.NewGuid()),
            MoveInDate.Create(DateOnly.FromDateTime(DateTime.UtcNow)),
            OccupantReference.Create(PersonId.New(), true),
            notes: null);

        tenancy.Close(
            EffectiveDate.Create(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))),
            TerminationReason.Create("Closed by test"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            tenancy.AddOccupant(PersonId.New(), false));

        Assert.Equal("Closed tenancy cannot be modified.", exception.Message);
        Assert.Contains(tenancy.DomainEvents, x => x is TenancyClosedDomainEvent);
    }
}
