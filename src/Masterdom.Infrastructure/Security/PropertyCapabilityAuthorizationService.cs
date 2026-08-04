using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;

namespace Masterdom.Infrastructure.Security;

internal sealed class PropertyCapabilityAuthorizationService : IPropertyCapabilityAuthorizationService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ICapabilityAuthorizationPolicyProvider _policyProvider;
    private readonly MasterdomDbContext _dbContext;

    public PropertyCapabilityAuthorizationService(
        ICurrentUserAccessor currentUserAccessor,
        ICapabilityAuthorizationPolicyProvider policyProvider,
        MasterdomDbContext dbContext)
    {
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
        _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public AuthorizationResult Authorize(AuthorizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var currentUser = _currentUserAccessor.GetCurrentUser();
        if (!currentUser.IsAuthenticated)
        {
            return AuthorizationResult.Challenge();
        }

        var policy = _policyProvider.GetPolicy(context.Operation);

        if (currentUser.IsInRole(MasterdomRoles.SuperUser))
        {
            return AuthorizationResult.Allowed();
        }

        if (policy.AllowsTenantSelf
            && currentUser.IsInRole(MasterdomRoles.Tenant)
            && context.PersonId.HasValue
            && currentUser.PersonId == context.PersonId)
        {
            return AuthorizationResult.Allowed();
        }

        if (policy.IsPropertyScoped)
        {
            if (policy.AllowsPropertyOwner
                && currentUser.IsInRole(MasterdomRoles.PropertyOwner)
                && currentUser.UserId.HasValue
                && OwnsResolvedProperty(currentUser.UserId.Value, context.PropertyId))
            {
                return AuthorizationResult.Allowed();
            }

            if (currentUser.IsInRole(MasterdomRoles.Manager)
                && currentUser.HasPermission(policy.RequiredPermission ?? string.Empty)
                && HasPropertyScope(currentUser, context.PropertyId))
            {
                return AuthorizationResult.Allowed();
            }

            return AuthorizationResult.Forbid();
        }

        if (policy.AllowsPropertyOwner && currentUser.IsInRole(MasterdomRoles.PropertyOwner))
        {
            return AuthorizationResult.Allowed();
        }

        if (currentUser.IsInRole(MasterdomRoles.Manager)
            && currentUser.HasPermission(policy.RequiredPermission ?? string.Empty))
        {
            return AuthorizationResult.Allowed();
        }

        return AuthorizationResult.Forbid();
    }

    private bool OwnsResolvedProperty(Guid userId, Guid? propertyId)
    {
        if (propertyId.HasValue)
        {
            return _dbContext.Properties.Any(x => x.Id.Value == propertyId.Value && x.OwnerId == userId);
        }

        return _dbContext.Properties.Any(x => x.OwnerId == userId);
    }

    private static bool HasPropertyScope(CurrentUser currentUser, Guid? propertyId)
    {
        if (propertyId.HasValue)
        {
            return currentUser.HasPropertyScope(propertyId.Value);
        }

        return currentUser.PropertyScopes.Count > 0;
    }
}
