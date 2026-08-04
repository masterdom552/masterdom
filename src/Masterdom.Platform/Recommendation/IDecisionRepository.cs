namespace Masterdom.Platform.Recommendation;

public interface IDecisionRepository
{
    void Save(Decision decision);

    Decision? Get(DecisionId decisionId);
}
