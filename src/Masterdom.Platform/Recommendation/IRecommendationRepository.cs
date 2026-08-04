namespace Masterdom.Platform.Recommendation;

public interface IRecommendationRepository
{
    void SaveBundle(RecommendationBundle bundle);

    RecommendationBundle? GetBundle(RecommendationBundleId bundleId);
}
