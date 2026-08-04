namespace Masterdom.Platform.Recommendation;

public sealed class InMemoryOptimizationSessionRepository : IOptimizationSessionRepository
{
    private readonly Dictionary<Guid, OptimizationSession> _sessions = new();

    public void Save(OptimizationSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _sessions[session.Id.Value] = session;
    }

    public OptimizationSession? Get(OptimizationSessionId sessionId)
    {
        ArgumentNullException.ThrowIfNull(sessionId);

        return _sessions.TryGetValue(sessionId.Value, out var session)
            ? session
            : null;
    }
}
