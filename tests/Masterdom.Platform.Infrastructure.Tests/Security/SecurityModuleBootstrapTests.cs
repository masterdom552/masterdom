using Masterdom.Core.Security;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

public sealed class SecurityModuleBootstrapTests
{
    [Fact]
    public void AddSecurityModule_ShouldRegisterSecurityRuntimeServices()
    {
        var services = new ServiceCollection();
        services.AddDbContext<MasterdomDbContext>(options =>
            options.UseInMemoryDatabase($"security-bootstrap-{Guid.NewGuid():N}"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Bearer:SigningKey"] = "security-module-bootstrap-signing-key-1234567890",
                ["Authentication:Bearer:Issuer"] = "masterdom-tests",
                ["Authentication:Bearer:Audience"] = "masterdom-tests"
            })
            .Build();

        services.AddSecurityModule(configuration);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var currentUserAccessor = scope.ServiceProvider.GetRequiredService<ICurrentUserAccessor>();
        var policyProvider = scope.ServiceProvider.GetRequiredService<ICapabilityAuthorizationPolicyProvider>();
        var authorizationService = scope.ServiceProvider.GetRequiredService<IPropertyCapabilityAuthorizationService>();
        var authenticationSchemeProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        Assert.Equal("HttpContextCurrentUserAccessor", currentUserAccessor.GetType().Name);
        Assert.NotNull(policyProvider);
        Assert.NotNull(authorizationService);
        Assert.NotNull(authenticationSchemeProvider);
    }
}
