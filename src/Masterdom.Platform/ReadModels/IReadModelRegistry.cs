namespace Masterdom.Platform.ReadModels;

public interface IReadModelRegistry
{
    IReadOnlyCollection<IReadModelProvider> GetProviders();

    IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels();

    IReadOnlyCollection<IReadModelProvider> ResolveProviders(string readModelKey);

    IReadOnlyCollection<ReadModelMetadata> ResolveMetadata(string readModelKey);
}
