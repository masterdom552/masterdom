using System.Text.Json;

namespace Masterdom.Platform.Configuration;

public sealed class BusinessConfigurationCatalog : IBusinessConfigurationCatalog
{
    private readonly IConfigurationResolver _configurationResolver;

    public BusinessConfigurationCatalog(IConfigurationResolver configurationResolver)
    {
        _configurationResolver = configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver));
    }

    public BusinessConfigurationAsset<TPayload> Resolve<TPayload>(ConfigurationKey key, ConfigurationResolutionRequest request)
    {
        var resolved = _configurationResolver.Resolve(key, request);
        var asset = JsonSerializer.Deserialize<BusinessConfigurationAsset<TPayload>>(resolved.Record.Value.Value);
        return asset ?? throw new InvalidOperationException($"Failed to deserialize business configuration asset for key '{key.Value}'.");
    }
}
