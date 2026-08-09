using Masterdom.Infrastructure;
using Masterdom.Modules.Settings.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Settings;

public sealed class SettingsRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveSettingsCapabilityBehaviorService()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetService<SettingsCapabilityBehaviorService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void SettingsCapabilityBehaviorService_ShouldExecuteThroughProductionRuntimePath()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<SettingsCapabilityBehaviorService>();
        var result = service.Execute();

        Assert.Equal("Settings", result.Capability);
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
