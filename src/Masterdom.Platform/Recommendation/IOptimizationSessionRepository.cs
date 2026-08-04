namespace Masterdom.Platform.Recommendation;

public interface IOptimizationSessionRepository
{
    void Save(OptimizationSession session);

    OptimizationSession? Get(OptimizationSessionId sessionId);
}
