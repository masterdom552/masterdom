using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Entities.Property.Events;

public sealed class PropertyTests
{
    [Fact]
    public void Create_ShouldInitializeProperty()
    {
        var property = Property.Create(
            new PropertyCode("SHOP-01"),
            new PropertyName("Main Retail Park"),
            PropertyType.Commercial);

        Assert.NotNull(property);
        Assert.Equal("SHOP-01", property.Code.Value);
        Assert.Equal("Main Retail Park", property.Name.Value);
        Assert.Equal(PropertyType.Commercial, property.Type);
        Assert.Equal(PropertyStatus.Active, property.Status);
        Assert.Equal(PropertySettings.Default, property.Settings);
        Assert.Empty(property.Units);
        Assert.Contains(property.DomainEvents, x => x is PropertyCreatedDomainEvent);
    }

    [Fact]
    public void CreateUnit_ShouldAddUnit_WhenUnitIsCreated()
    {
        var property = Property.Create(
            new PropertyCode("OFFICE-01"),
            new PropertyName("Corporate Campus"),
            PropertyType.Commercial);

        var unit = property.CreateUnit(
            new UnitCode("SUITE-100"),
            "Suite 100",
            UnitType.Office);

        Assert.Single(property.Units);
        Assert.Contains(unit, property.Units);
    }

    [Fact]
    public void AddUnit_ShouldThrow_WhenUnitAlreadyExists()
    {
        var property = Property.Create(
            new PropertyCode("WAREHOUSE-01"),
            new PropertyName("Logistics Hub"),
            PropertyType.Warehouse);

        var unit = (Unit)Activator.CreateInstance(
            typeof(Unit),
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic,
            binder: null,
            args: [
                UnitId.New(),
                new UnitCode("DOCK-1"),
                new UnitName("Dock 1"),
                UnitType.Warehouse,
                OccupancyStatus.Vacant,
                new Capacity(2)
            ],
            culture: null)!;

        property.AddUnit(unit);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            property.AddUnit(unit));

        Assert.Equal($"Unit '{unit.Id}' already exists.", exception.Message);
        Assert.Single(property.Units);
    }

    [Fact]
    public void CreateUnit_ShouldThrow_WhenDuplicateCodeExists()
    {
        var property = Property.Create(
            new PropertyCode("AP-01"),
            new PropertyName("Atlas Plaza"),
            PropertyType.MixedUse);

        property.CreateUnit(new UnitCode("SUITE-1"), "Suite 1", UnitType.Office);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            property.CreateUnit(new UnitCode("SUITE-1"), "Suite 1B", UnitType.Office));

        Assert.Equal("A unit with code 'SUITE-1' already exists.", exception.Message);
    }

    [Fact]
    public void Archive_ShouldThrow_WhenPropertyContainsUnits()
    {
        var property = Property.Create(
            new PropertyCode("TOWER-01"),
            new PropertyName("North Tower"),
            PropertyType.Tower);

        property.CreateUnit(new UnitCode("F1-001"), "Floor 1 Unit 1", UnitType.Office);

        var exception = Assert.Throws<InvalidOperationException>(() => property.Archive());

        Assert.Equal("A property containing units cannot be archived.", exception.Message);
    }

    [Fact]
    public void SetEffectivePeriod_ShouldThrow_WhenFromIsAfterTo()
    {
        var property = Property.Create(
            new PropertyCode("LAND-01"),
            new PropertyName("South Lot"),
            PropertyType.Land);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            property.SetEffectivePeriod(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(-1)));

        Assert.Equal("EffectiveFromUtc cannot be after EffectiveToUtc.", exception.Message);
    }

    [Fact]
    public void AddRelationship_ShouldThrow_WhenTargetIsSelf()
    {
        var property = Property.Create(
            new PropertyCode("PARENT-01"),
            new PropertyName("Parent Property"),
            PropertyType.Residential);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            property.AddRelationship(new PropertyRelationship(property.Id, PropertyRelationshipType.ParentChild)));

        Assert.Equal("A property cannot reference itself in relationships.", exception.Message);
    }

    [Fact]
    public void UpsertMetadata_ShouldReplaceExistingByKey()
    {
        var property = Property.Create(
            new PropertyCode("META-01"),
            new PropertyName("Meta Building"),
            PropertyType.Commercial);

        property.UpsertMetadata(new PropertyMetadata("RiskLevel", "Low"));
        property.UpsertMetadata(new PropertyMetadata("risklevel", "Medium"));

        var metadata = Assert.Single(property.Metadata);
        Assert.Equal("risklevel", metadata.Key);
        Assert.Equal("Medium", metadata.Value);
    }

    [Fact]
    public void ConfigureSettings_ShouldApplyValueObject()
    {
        var property = Property.Create(
            new PropertyCode("CFG-01"),
            new PropertyName("Configurable Site"),
            PropertyType.Commercial);

        var settings = new PropertySettings("Asia/Karachi", "PKR", false);

        property.ConfigureSettings(settings);

        Assert.Equal(settings, property.Settings);
    }

    [Fact]
    public void DomainEvents_ShouldTrackLifecycleChanges()
    {
        var property = Property.Create(
            new PropertyCode("EVT-01"),
            new PropertyName("Event Site"),
            PropertyType.Residential);

        property.Rename(new PropertyName("Event Site Updated"));
        property.Deactivate();

        Assert.Contains(property.DomainEvents, x => x is PropertyCreatedDomainEvent);
        Assert.Contains(property.DomainEvents, x => x is PropertyRenamedDomainEvent);
        Assert.Contains(property.DomainEvents, x => x is PropertyStatusChangedDomainEvent evt && evt.Status == PropertyStatus.Inactive);

        property.ClearDomainEvents();
        Assert.Empty(property.DomainEvents);
    }

    [Fact]
    public void RemoveUnit_ShouldReturnTrue_WhenUnitExists()
    {
        var property = Property.Create(
            new PropertyCode("APARTMENT-01"),
            new PropertyName("Residential Tower"),
            PropertyType.Residential);

        var unit = property.CreateUnit(
            new UnitCode("APT-1A"),
            "Apartment 1A",
            UnitType.Room);
        var removed = property.RemoveUnit(unit.Id);

        Assert.True(removed);
        Assert.Empty(property.Units);
        Assert.Contains(property.DomainEvents, x => x is UnitRemovedDomainEvent);
    }

    [Fact]
    public void RemoveUnit_ShouldReturnFalse_WhenUnitDoesNotExist()
    {
        var property = Property.Create(
            new PropertyCode("APARTMENT-02"),
            new PropertyName("Residential Tower"),
            PropertyType.Residential);

        var removed = property.RemoveUnit(UnitId.New());

        Assert.False(removed);
    }
}
