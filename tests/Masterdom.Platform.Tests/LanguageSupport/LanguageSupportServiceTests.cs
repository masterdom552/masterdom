using Masterdom.Platform.Configuration;
using Masterdom.Platform.LanguageSupport;

namespace Masterdom.Platform.Tests.LanguageSupport;

public sealed class LanguageSupportServiceTests
{
    [Fact]
    public void ResolveText_ShouldUseFallbackChain_AndParameterSubstitution()
    {
        var service = CreateService();

        service.SwitchLanguage(new LanguageResolutionRequest(
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "pa-IN",
            RequestedLocale: "pa-IN",
            ResourceCatalogReference: null));

        var text = service.ResolveText("Platform.Greeting", new Dictionary<string, string>
        {
            ["name"] = "Sam"
        });

        Assert.Equal("Hello Sam", text);
    }

    [Fact]
    public void ResolvePluralText_ShouldUseSingularAndPluralForms()
    {
        var service = CreateService();
        service.SwitchLanguage(new LanguageResolutionRequest(
            new ConfigurationResolutionRequest
            {
                ModuleId = "reporting",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "en-GB",
            RequestedLocale: "en-GB",
            ResourceCatalogReference: null));

        Assert.Equal("1 item", service.ResolvePluralText("Platform.Items", 1));
        Assert.Equal("5 items", service.ResolvePluralText("Platform.Items", 5));
    }

    [Fact]
    public void FormattingAndParsing_ShouldBeLocaleAware()
    {
        var service = CreateService();
        service.SwitchLanguage(new LanguageResolutionRequest(
            new ConfigurationResolutionRequest
            {
                ModuleId = "documents",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "en-US",
            RequestedLocale: "en-US",
            ResourceCatalogReference: null));

        Assert.Equal("2026-08-03", service.FormatDate(new DateTime(2026, 8, 3)));
        Assert.Equal("13:45", service.FormatTime(new TimeOnly(13, 45)));
        Assert.Equal("1,234.50", service.FormatNumber(1234.5m));
        Assert.Equal("$1,234.50", service.FormatCurrency(1234.5m));
        Assert.Equal(new DateTime(2026, 8, 3), service.ParseDate("2026-08-03"));
        Assert.Equal(new TimeOnly(13, 45), service.ParseTime("13:45"));
        Assert.Equal(1234.5m, service.ParseNumber("1,234.50"));
    }

    [Fact]
    public void SwitchLanguage_ShouldUpdateRuntimeLanguageWithoutRestart()
    {
        var service = CreateService();

        service.SwitchLanguage(new LanguageResolutionRequest(
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "en-US",
            RequestedLocale: "en-US",
            ResourceCatalogReference: null));

        var firstCulture = service.CurrentSettings.Culture;

        service.SwitchLanguage(new LanguageResolutionRequest(
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "en-GB",
            RequestedLocale: "en-GB",
            ResourceCatalogReference: null));

        Assert.Equal("en-US", firstCulture);
        Assert.Equal("en-GB", service.CurrentSettings.Culture);
    }

    [Fact]
    public void ResolveText_ShouldRejectFlatResourceKeys()
    {
        var service = CreateService();
        service.SwitchLanguage(new LanguageResolutionRequest(
            new ConfigurationResolutionRequest
            {
                ModuleId = "billing",
                AsOfUtc = DateTime.UtcNow
            },
            RequestedCulture: "en-US",
            RequestedLocale: "en-US",
            ResourceCatalogReference: null));

        Assert.Throws<InvalidOperationException>(() => service.ResolveText("Invoice"));
    }

    private static DefaultLanguageSupportService CreateService()
    {
        var resources = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["en-US"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Platform.Greeting"] = "Hello {{name}}",
                ["Platform.Items.one"] = "{{count}} item",
                ["Platform.Items.other"] = "{{count}} items"
            },
            ["en-GB"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Platform.Greeting"] = "Hello {{name}}"
            }
        };

        return new DefaultLanguageSupportService(
            [new DefaultLanguageResourceProvider(resources)],
            new FixedLanguageSettingsResolver(),
            new AsyncLocalLanguageContextAccessor(),
            new DefaultLanguageFormatterProvider());
    }

    private sealed class FixedLanguageSettingsResolver : ILanguageSettingsResolver
    {
        public LanguageSettings Resolve(LanguageResolutionRequest request)
        {
            return request.RequestedCulture switch
            {
                "pa-IN" => new LanguageSettings(
                    Culture: "pa-IN",
                    Locale: "pa-IN",
                    FallbackCultures: ["hi-IN", "en-US"],
                    DateFormat: "dd/MM/yyyy",
                    TimeFormat: "HH:mm",
                    NumberFormat: "N2",
                    CurrencyFormat: "C2",
                    UseTwentyFourHourTime: true),
                "en-GB" => new LanguageSettings(
                    Culture: "en-GB",
                    Locale: "en-GB",
                    FallbackCultures: ["en-US"],
                    DateFormat: "dd/MM/yyyy",
                    TimeFormat: "HH:mm",
                    NumberFormat: "N2",
                    CurrencyFormat: "C2",
                    UseTwentyFourHourTime: true),
                _ => LanguageSettings.EnglishBaseline()
            };
        }
    }
}
