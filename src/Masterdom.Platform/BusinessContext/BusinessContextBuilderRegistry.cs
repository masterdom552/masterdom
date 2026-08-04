namespace Masterdom.Platform.BusinessContext;

/// <summary>
/// Discovers and orders Business Context providers.
/// </summary>
public sealed class BusinessContextBuilderRegistry
{
    private readonly List<IBusinessContextProvider> _providers;

    public BusinessContextBuilderRegistry(IEnumerable<IBusinessContextProvider>? providers = null)
    {
        _providers = providers?.ToList() ?? [];
    }

    public IReadOnlyList<IBusinessContextProvider> GetOrderedProviders()
    {
        return _providers
            .OrderBy(provider => provider.Order)
            .ThenByDescending(provider => provider.Priority)
            .ThenBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
