namespace Masterdom.Platform.ImportExport;

public sealed class LookupProviderRegistry : ILookupProviderRegistry
{
    private readonly IReadOnlyDictionary<string, ILookupProvider> _providers;

    public LookupProviderRegistry(IEnumerable<ILookupProvider>? providers = null)
    {
        var resolved = providers?.ToList() ?? [new PassthroughLookupProvider()];
        _providers = resolved.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    public ILookupProvider Resolve(string name)
    {
        return _providers.TryGetValue(name, out var provider)
            ? provider
            : _providers[PassthroughLookupProvider.ProviderName];
    }
}
