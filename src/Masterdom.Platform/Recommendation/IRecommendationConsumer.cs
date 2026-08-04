using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;

namespace Masterdom.Platform.Recommendation;

/// <summary>
/// Internal recommendation-platform extension point for reacting to generated recommendations.
/// </summary>
public interface IRecommendationConsumer
{
    string Name { get; }

    int Order { get; }

    int Priority { get; }

    bool IsOptional { get; }

    RecommendationConsumerResult Consume(RecommendationConsumerExecutionContext context);
}
