using System.Text.Json;
using Masterdom.Platform.Configuration;

namespace Masterdom.Platform.LanguageSupport;

public sealed class ConfigurationLanguageSettingsResolver : ILanguageSettingsResolver
{
    private static readonly ConfigurationKey SettingsKey = new("platform.language.settings");
    private readonly IConfigurationResolver _configurationResolver;

    public ConfigurationLanguageSettingsResolver(IConfigurationResolver configurationResolver)
    {
        _configurationResolver = configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver));
    }

    public LanguageSettings Resolve(LanguageResolutionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = TryResolveFromConfiguration(request.ConfigurationRequest)
            ?? LanguageSettings.EnglishBaseline();

        return settings with
        {
            Culture = request.RequestedCulture ?? settings.Culture,
            Locale = request.RequestedLocale ?? settings.Locale
        };
    }

    private LanguageSettings? TryResolveFromConfiguration(ConfigurationResolutionRequest request)
    {
        try
        {
            var resolved = _configurationResolver.Resolve(SettingsKey, request);
            var settings = JsonSerializer.Deserialize<LanguageSettings>(resolved.Record.Value.Value);
            return settings;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
