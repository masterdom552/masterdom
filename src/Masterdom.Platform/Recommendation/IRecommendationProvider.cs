using Masterdom.Platform.BusinessContext;
using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;

namespace Masterdom.Platform.Recommendation;

public interface IRecommendationProvider
{
    string Name { get; }

    int Order { get; }

    int Priority { get; }

    bool IsOptional { get; }

    IReadOnlyList<Recommendation> Provide(BusinessContextModel context, OptimizationSession session);
}
