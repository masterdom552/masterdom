namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationProviderRegistry
{
    private readonly IReadOnlyList<IRecommendationProvider> _providers;

    public RecommendationProviderRegistry(IEnumerable<IRecommendationProvider>? providers = null)
    {
        _providers = providers?.ToArray() ?? Array.Empty<IRecommendationProvider>();
    }

    public IReadOnlyList<IRecommendationProvider> GetOrderedProviders()
    {
        return _providers
            .OrderBy(x => x.Order)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
