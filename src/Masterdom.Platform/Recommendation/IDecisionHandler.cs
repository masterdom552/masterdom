using Masterdom.Platform.BusinessContext;
using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;

namespace Masterdom.Platform.Recommendation;

public interface IDecisionHandler
{
    string Name { get; }

    int Order { get; }

    int Priority { get; }

    Decision Handle(Decision decision, RecommendationBundle bundle, BusinessContextModel context);
}
