using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Platform.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.Notifications;

public sealed class NotificationRegistryMetadataTests
{
    [Fact]
    public void NotificationRegistry_ShouldBeMetadataDriven()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var registry = scope.ServiceProvider.GetRequiredService<INotificationRegistry>();
        var registrations = registry.GetAll();

        Assert.NotEmpty(registrations);
        Assert.All(registrations, registration =>
        {
            Assert.False(string.IsNullOrWhiteSpace(registration.EventCode));
            Assert.True(registration.Version > 0);
            Assert.False(string.IsNullOrWhiteSpace(registration.ReadModelKey));
            Assert.False(string.IsNullOrWhiteSpace(registration.TemplateCode));
            Assert.False(string.IsNullOrWhiteSpace(registration.RecipientResolver));
            Assert.NotEmpty(registration.DeliveryChannels);
            Assert.True(registration.RetryMaxAttempts > 0);
            Assert.False(string.IsNullOrWhiteSpace(registration.SchedulingPolicy));
        });
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"notification-registry-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }
}
