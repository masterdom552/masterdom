namespace Masterdom.Platform.Recommendation;

public sealed class InMemoryRecommendationRepository : IRecommendationRepository
{
    private readonly Dictionary<Guid, RecommendationBundle> _bundles = new();

    public void SaveBundle(RecommendationBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        _bundles[bundle.Id.Value] = bundle;
    }

    public RecommendationBundle? GetBundle(RecommendationBundleId bundleId)
    {
        ArgumentNullException.ThrowIfNull(bundleId);

        return _bundles.TryGetValue(bundleId.Value, out var bundle)
            ? bundle
            : null;
    }
}
