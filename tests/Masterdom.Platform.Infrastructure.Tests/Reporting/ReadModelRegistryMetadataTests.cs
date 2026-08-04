using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Platform.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Reporting;

public sealed class ReadModelRegistryMetadataTests
{
    [Fact]
    public void ReadModelMetadata_ShouldContainRequiredRegistrationFields()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var registry = scope.ServiceProvider.GetRequiredService<IReadModelRegistry>();
        var metadata = registry.GetRegisteredReadModels();

        Assert.NotEmpty(metadata);
        Assert.All(metadata, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(item.ReadModelKey));
            Assert.True(item.Version > 0);
            Assert.False(string.IsNullOrWhiteSpace(item.Provider));
            Assert.False(string.IsNullOrWhiteSpace(item.Description));
            Assert.NotEmpty(item.ConsumerCompatibility);
            Assert.NotNull(item.SupportedParameters);
            Assert.NotEmpty(item.OutputSchema);
        });
    }

    [Fact]
    public void ReportRegistry_ShouldResolveRegistrationsWithoutHardcodedApplicationMapping()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var registry = scope.ServiceProvider.GetRequiredService<IReportReadModelRegistry>();
        var registrations = registry.GetRegistrations();

        Assert.NotEmpty(registrations);
        Assert.All(registrations, registration =>
        {
            Assert.False(string.IsNullOrWhiteSpace(registration.ReportCode));
            Assert.NotEmpty(registration.ReadModelKeys);
            Assert.NotEmpty(registration.OutputSchema);
        });
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"readmodel-registry-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }
}
