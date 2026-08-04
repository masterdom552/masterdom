using Masterdom.Infrastructure;
using Masterdom.Platform.LanguageSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.LanguageSupport;

public sealed class LanguageSupportRuntimeCompositionTests
{
    [Fact]
    public void Runtime_ShouldResolveLanguageSupportService()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ILanguageSupportService>();
        service.SwitchLanguage(new LanguageResolutionRequest(
            new Masterdom.Platform.Configuration.ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "en-US",
            RequestedLocale: "en-US"));

        Assert.Equal("en-US", service.CurrentSettings.Culture);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<Masterdom.Infrastructure.Persistence.MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"language-support-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        return services.BuildServiceProvider(validateScopes: true);
    }
}
