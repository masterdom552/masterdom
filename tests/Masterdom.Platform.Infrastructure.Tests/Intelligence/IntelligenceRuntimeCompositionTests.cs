using Masterdom.Infrastructure;
using Masterdom.Modules.Intelligence.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Intelligence;

public sealed class IntelligenceRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveIntelligenceCapabilityBehaviorService()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetService<IntelligenceCapabilityBehaviorService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void IntelligenceCapabilityBehaviorService_ShouldExecuteThroughProductionRuntimePath()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IntelligenceCapabilityBehaviorService>();
        var result = service.Execute();

        Assert.Equal("Intelligence", result.Capability);
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
