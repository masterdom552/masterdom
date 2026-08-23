using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence.Identity;

namespace Masterdom.Infrastructure.Security;

internal sealed class ActiveDelegationsProvider : IActiveDelegationsProvider
{
    private readonly IDelegatedAuthorityRepository _delegatedAuthorityRepository;

    public ActiveDelegationsProvider(IDelegatedAuthorityRepository delegatedAuthorityRepository)
    {
        _delegatedAuthorityRepository = delegatedAuthorityRepository
            ?? throw new ArgumentNullException(nameof(delegatedAuthorityRepository));
    }

    public Task<IReadOnlyCollection<DelegatedAuthority>> GetActiveDelegationsAsync(Guid userId, DateTime utcNow)
    {
        return _delegatedAuthorityRepository.GetActiveDelegationsAsync(userId, utcNow);
    }
}
