using Masterdom.Platform.ReadModels;

namespace Masterdom.Infrastructure.Persistence.ReadModels;

internal sealed class ReadModelProjectionOrchestrator : IReadModelProjectionOrchestrator
{
    private readonly IReadModelRegistry _registry;

    public ReadModelProjectionOrchestrator(IReadModelRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public IReadOnlyCollection<ReadModelProjectionResult> Project(string readModelKey, ReadModelProjectionRequest request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(readModelKey);
        ArgumentNullException.ThrowIfNull(request);

        var providers = _registry.ResolveProviders(readModelKey);
        if (providers.Count == 0)
        {
            throw new InvalidOperationException($"No read model provider is registered for '{readModelKey}'.");
        }

        var results = new List<ReadModelProjectionResult>();

        foreach (var provider in providers)
        {
            var metadata = provider.GetRegisteredReadModels()
                .First(x => x.ReadModelKey.Equals(readModelKey, StringComparison.OrdinalIgnoreCase));
            var records = provider.Project(readModelKey, request);

            results.Add(new ReadModelProjectionResult(metadata, records, DateTime.UtcNow));
        }

        return results;
    }
}
