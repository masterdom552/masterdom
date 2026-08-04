namespace Masterdom.Platform.Configuration;

/// <summary>
/// Resolves effective configuration values for a key and context.
/// </summary>
public interface IConfigurationResolver
{
    ConfigurationResolutionResult Resolve(
        ConfigurationKey key,
        ConfigurationResolutionRequest request);
}
