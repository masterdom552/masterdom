using Masterdom.Modules.Properties.Domain.Entities.Property;
using PropertyEntity = Masterdom.Modules.Properties.Domain.Entities.Property.Property;

namespace Masterdom.Core.Tests.Property;

public sealed class StockLocationTests
{
    [Fact]
    public void AddStockLocation_WithValidName_ShouldCreateActiveLocation()
    {
        var property = CreateProperty();

        var location = property.AddStockLocation("Storage Room");

        Assert.Equal("Storage Room", location.Name);
        Assert.Null(location.Code);
        Assert.True(location.IsActive);
        Assert.Equal(property.Id.Value, location.PropertyId.Value);
        Assert.Single(property.StockLocations);
    }

    [Fact]
    public void AddStockLocation_WithNameAndCode_ShouldCreateWithCode()
    {
        var property = CreateProperty();

        var location = property.AddStockLocation("General", "GENERAL");

        Assert.Equal("General", location.Name);
        Assert.Equal("GENERAL", location.Code);
    }

    [Fact]
    public void AddStockLocation_TrimsNameAndCode()
    {
        var property = CreateProperty();

        var location = property.AddStockLocation("  Storage  ", "  GEN  ");

        Assert.Equal("Storage", location.Name);
        Assert.Equal("GEN", location.Code);
    }

    [Fact]
    public void AddStockLocation_WithNullOrWhitespaceName_ShouldThrow()
    {
        var property = CreateProperty();

        Assert.Throws<ArgumentException>(() => property.AddStockLocation("   "));
        Assert.Empty(property.StockLocations);
    }

    [Fact]
    public void AddStockLocation_WithNameExceeding200Chars_ShouldThrow()
    {
        var property = CreateProperty();
        var longName = new string('X', 201);

        Assert.Throws<ArgumentException>(() => property.AddStockLocation(longName));
        Assert.Empty(property.StockLocations);
    }

    [Fact]
    public void AddStockLocation_WithCodeExceeding64Chars_ShouldThrow()
    {
        var property = CreateProperty();
        var longCode = new string('X', 65);

        Assert.Throws<ArgumentException>(() => property.AddStockLocation("Valid", longCode));
        Assert.Empty(property.StockLocations);
    }

    [Fact]
    public void AddStockLocation_WithDuplicateName_ShouldThrow()
    {
        var property = CreateProperty();
        property.AddStockLocation("Storage Room");

        Assert.Throws<InvalidOperationException>(() => property.AddStockLocation("Storage Room"));
        Assert.Single(property.StockLocations);
    }

    [Fact]
    public void AddStockLocation_SameNameDifferentProperties_ShouldSucceed()
    {
        var property1 = CreateProperty("PROP-A");
        var property2 = CreateProperty("PROP-B");

        property1.AddStockLocation("General");
        property2.AddStockLocation("General");

        Assert.Single(property1.StockLocations);
        Assert.Single(property2.StockLocations);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var property = CreateProperty();
        var location = property.AddStockLocation("Storage");

        location.Deactivate();

        Assert.False(location.IsActive);
    }

    [Fact]
    public void Deactivate_IsIdempotent()
    {
        var property = CreateProperty();
        var location = property.AddStockLocation("Storage");

        location.Deactivate();
        location.Deactivate();

        Assert.False(location.IsActive);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var property = CreateProperty();
        var location = property.AddStockLocation("Storage");
        location.Deactivate();

        location.Activate();

        Assert.True(location.IsActive);
    }

    [Fact]
    public void Activate_IsIdempotent()
    {
        var property = CreateProperty();
        var location = property.AddStockLocation("Storage");

        location.Activate();
        location.Activate();

        Assert.True(location.IsActive);
    }

    [Fact]
    public void StockLocation_HasStableIdentity()
    {
        var property = CreateProperty();
        var location = property.AddStockLocation("Warehouse");

        Assert.NotEqual(Guid.Empty, location.Id.Value);
    }

    private static PropertyEntity CreateProperty(string code = "TEST-001")
    {
        return PropertyEntity.Create(
            new PropertyCode(code),
            new PropertyName("Test Property"),
            PropertyType.Commercial);
    }
}
