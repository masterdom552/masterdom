namespace Masterdom.Platform.Configuration;

public interface IBusinessConfigurationCatalog
{
    BusinessConfigurationAsset<TPayload> Resolve<TPayload>(ConfigurationKey key, ConfigurationResolutionRequest request);
}
