using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence.Identity;
using Masterdom.Modules.Security.Domain.Repositories;

namespace Masterdom.Modules.Security.Application.Services;

/// <summary>
/// Production implementation of <see cref="ILoginAuthorityResolver"/>.
/// Orchestrates the same, already-trusted authority chain CAP-018 uses
/// (<see cref="IDirectAuthorityProvider"/>, active delegations,
/// <see cref="EffectiveAuthorityResolver"/>) -- it does not recompute
/// authority independently.
/// </summary>
public sealed class LoginAuthorityResolver : ILoginAuthorityResolver
{
    private readonly IDirectAuthorityProvider _directAuthorityProvider;
    private readonly IDelegatedAuthorityRepository _delegatedAuthorityRepository;
    private readonly EffectiveAuthorityResolver _effectiveAuthorityResolver;
    private readonly IRoleRepository _roleRepository;

    public LoginAuthorityResolver(
        IDirectAuthorityProvider directAuthorityProvider,
        IDelegatedAuthorityRepository delegatedAuthorityRepository,
        EffectiveAuthorityResolver effectiveAuthorityResolver,
        IRoleRepository roleRepository)
    {
        _directAuthorityProvider = directAuthorityProvider ?? throw new ArgumentNullException(nameof(directAuthorityProvider));
        _delegatedAuthorityRepository = delegatedAuthorityRepository ?? throw new ArgumentNullException(nameof(delegatedAuthorityRepository));
        _effectiveAuthorityResolver = effectiveAuthorityResolver ?? throw new ArgumentNullException(nameof(effectiveAuthorityResolver));
        _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
    }

    public async Task<LoginAuthorityClaims> ResolveAsync(
        Guid userId,
        IReadOnlyCollection<Guid> directPropertyScopes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(directPropertyScopes);

        var directAuthority = await _directAuthorityProvider.GetDirectAuthorityAsync(
            userId,
            directPropertyScopes,
            cancellationToken);

        if (directAuthority is null)
        {
            return LoginAuthorityClaims.None(directPropertyScopes);
        }

        var utcNow = DateTime.UtcNow;
        var activeDelegations = await _delegatedAuthorityRepository.GetActiveDelegationsAsync(userId, utcNow);

        var effectiveAuthority = _effectiveAuthorityResolver.Resolve(
            userId,
            directAuthority,
            activeDelegations,
            utcNow);

        var roleCodes = new List<string>();
        foreach (var roleId in effectiveAuthority.Roles)
        {
            var role = _roleRepository.GetById(roleId);
            if (role is not null)
            {
                roleCodes.Add(role.Code.Value);
            }
        }

        return new LoginAuthorityClaims(
            roleCodes,
            effectiveAuthority.Permissions,
            effectiveAuthority.PropertyScopes,
            effectiveAuthority.EffectiveLevel);
    }
}
