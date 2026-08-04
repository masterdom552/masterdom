using Masterdom.Infrastructure;
using Masterdom.Platform.BusinessContext;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.BusinessContext;

public sealed class BusinessContextRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveBusinessContextBuilder()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var builder = scope.ServiceProvider.GetService<IBusinessContextBuilder>();

        Assert.NotNull(builder);
    }

    [Fact]
    public void RuntimeComposition_ShouldExecuteRegisteredProviders()
    {
        using var provider = BuildProvider(services =>
        {
            services.AddScoped<IBusinessContextProvider>(_ =>
                new StubProvider("property", order: 10, priority: 1));
            services.AddScoped<IBusinessContextProvider>(_ =>
                new StubProvider("billing", order: 20, priority: 10));
        });

        using var scope = provider.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IBusinessContextBuilder>();

        var result = builder.Build(new BusinessContextRequest(
            effectiveDateUtc: new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
            configurationVersion: "cfg-runtime-v1"));

        Assert.Equal(["property", "billing"], result.Context.Metadata.ProviderExecutionOrder);
        Assert.Equal(2, result.Context.Snapshots.Count);
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? registerProviders = null)
    {
        var services = new ServiceCollection();

        registerProviders?.Invoke(services);
        services.AddPropertyBusinessCapabilityRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class StubProvider : IBusinessContextProvider
    {
        public StubProvider(string name, int order, int priority)
        {
            Name = name;
            Order = order;
            Priority = priority;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => true;

        public BusinessContextProviderResult Provide(BusinessContextRequest request)
        {
            return new BusinessContextProviderResult(
                snapshots:
                [
                    new BusinessContextSnapshot(
                        Key: $"{Name}.snapshot",
                        Payload: new { request.EffectiveDateUtc, request.ConfigurationVersion })
                ],
                references: Array.Empty<BusinessContextReference>());
        }
    }
}
