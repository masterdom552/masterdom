namespace Masterdom.Platform.ReadModels;

public interface IReadModelProjectionOrchestrator
{
    IReadOnlyCollection<ReadModelProjectionResult> Project(string readModelKey, ReadModelProjectionRequest request);
}
