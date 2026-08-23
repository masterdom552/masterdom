using Masterdom.Infrastructure;
using Masterdom.Modules.Authentication.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Authentication;

public sealed class AuthenticationRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveAuthenticationCapabilityBehaviorService()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetService<AuthenticationCapabilityBehaviorService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void AuthenticationCapabilityBehaviorService_ShouldExecuteThroughProductionRuntimePath()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<AuthenticationCapabilityBehaviorService>();
        var result = service.Execute();

        Assert.Equal("Authentication", result.Capability);
        Assert.Equal("Runtime", result.ExecutionPath);
        Assert.True(result.IsSupported);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddPropertyBusinessCapabilityRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }
}
