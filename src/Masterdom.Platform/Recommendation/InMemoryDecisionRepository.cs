namespace Masterdom.Platform.Recommendation;

public sealed class InMemoryDecisionRepository : IDecisionRepository
{
    private readonly Dictionary<Guid, Decision> _decisions = new();

    public void Save(Decision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        _decisions[decision.Id.Value] = decision;
    }

    public Decision? Get(DecisionId decisionId)
    {
        ArgumentNullException.ThrowIfNull(decisionId);

        return _decisions.TryGetValue(decisionId.Value, out var decision)
            ? decision
            : null;
    }
}
