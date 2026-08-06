using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Masterdom.Infrastructure.Security;

public static class SecurityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSecurityInfrastructureRuntime(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<Masterdom.Core.Security.ICurrentUserAccessor, AnonymousCurrentUserAccessor>();
        services.TryAddScoped<Masterdom.Core.Security.ICapabilityAuthorizationPolicyProvider, DefaultCapabilityAuthorizationPolicyProvider>();
        services.TryAddScoped<Masterdom.Core.Security.IPropertyCapabilityAuthorizationService, PropertyCapabilityAuthorizationService>();
        services.TryAddScoped<IRequestAuthorizationService, RequestAuthorizationService>();

        return services;
    }
}
