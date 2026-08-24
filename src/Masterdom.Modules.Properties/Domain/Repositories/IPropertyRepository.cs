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

    /// <summary>
    /// Lists properties directly owned by the given owner, unconditionally
    /// -- not subject to the caller's own read-access filter. Intended for
    /// server-side scope derivation (e.g. at authentication time), not for
    /// general request-time reads.
    /// </summary>
    IReadOnlyCollection<Property> ListOwnedBy(Guid ownerId);

    void Add(Property property);

    void Update(Property property);
}
