using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Domain.Repositories;

/// <summary>
/// Provides aggregate persistence boundary for properties.
/// </summary>
public interface IPropertyRepository
{
    Property? GetById(PropertyId id);

    Property? GetByCode(PropertyCode code);

    IReadOnlyCollection<Unit> ListUnits(PropertyId propertyId);

    IReadOnlyCollection<Property> Search(string? codeContains, int take);

    void Add(Property property);

    void Update(Property property);
}
