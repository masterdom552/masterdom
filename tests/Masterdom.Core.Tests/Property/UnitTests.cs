using Masterdom.Modules.Properties.Domain.Entities.Property;

public sealed class UnitTests
{
    [Fact]
    public void SetCapacity_ShouldUpdateCapacity()
    {
        var property = Property.Create(
            new PropertyCode("UNIT-01"),
            new PropertyName("Unit Test Property"),
            PropertyType.Commercial);

        var unit = property.CreateUnit(
            new UnitCode("SUITE-500"),
            "Suite 500",
            UnitType.Office,
            new Capacity(3));

        unit.SetCapacity(new Capacity(5));

        Assert.Equal(5, unit.Capacity.Value);
    }

    [Fact]
    public void AssignParentUnit_ShouldThrow_WhenParentIsSelf()
    {
        var property = Property.Create(
            new PropertyCode("UNIT-02"),
            new PropertyName("Unit Hierarchy Property"),
            PropertyType.Commercial);

        var unit = property.CreateUnit(
            new UnitCode("PARENT-1"),
            "Parent 1",
            UnitType.Office);

        var exception = Assert.Throws<InvalidOperationException>(() => unit.AssignParentUnit(unit.Id));

        Assert.Equal("A unit cannot reference itself as parent.", exception.Message);
    }

    [Fact]
    public void SetDisplayOrder_ShouldThrow_WhenNegative()
    {
        var property = Property.Create(
            new PropertyCode("UNIT-03"),
            new PropertyName("Display Property"),
            PropertyType.Commercial);

        var unit = property.CreateUnit(
            new UnitCode("ORDER-1"),
            "Order 1",
            UnitType.Office);

        Assert.Throws<ArgumentOutOfRangeException>(() => unit.SetDisplayOrder(-1));
    }
}
