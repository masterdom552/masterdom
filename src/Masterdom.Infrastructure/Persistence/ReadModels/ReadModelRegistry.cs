using Masterdom.Platform.ReadModels;

namespace Masterdom.Infrastructure.Persistence.ReadModels;

internal sealed class ReadModelRegistry : IReadModelRegistry
{
    private readonly IReadOnlyCollection<IReadModelProvider> _providers;

    public ReadModelRegistry(IEnumerable<IReadModelProvider> providers)
    {
        _providers = providers?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(providers));
    }

    public IReadOnlyCollection<IReadModelProvider> GetProviders() => _providers;

    public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels()
    {
        return _providers.SelectMany(x => x.GetRegisteredReadModels()).ToList();
    }

    public IReadOnlyCollection<IReadModelProvider> ResolveProviders(string readModelKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(readModelKey);

        return _providers
            .Where(provider => provider.GetRegisteredReadModels().Any(model =>
                model.ReadModelKey.Equals(readModelKey, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public IReadOnlyCollection<ReadModelMetadata> ResolveMetadata(string readModelKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(readModelKey);

        return _providers
            .SelectMany(x => x.GetRegisteredReadModels())
            .Where(x => x.ReadModelKey.Equals(readModelKey, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
