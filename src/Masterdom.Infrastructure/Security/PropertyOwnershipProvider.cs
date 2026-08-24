using Masterdom.Core.Security;
using Masterdom.Modules.Properties.Domain.Repositories;

namespace Masterdom.Infrastructure.Security;

internal sealed class PropertyOwnershipProvider : IPropertyOwnershipProvider
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyOwnershipProvider(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository
            ?? throw new ArgumentNullException(nameof(propertyRepository));
    }

    public Task<IReadOnlyCollection<Guid>> GetOwnedPropertyIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var ownedPropertyIds = _propertyRepository
            .ListOwnedBy(userId)
            .Select(x => x.Id.Value)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<Guid>>(ownedPropertyIds);
    }
}
