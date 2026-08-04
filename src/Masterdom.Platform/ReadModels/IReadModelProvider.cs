namespace Masterdom.Platform.ReadModels;

public interface IReadModelProvider
{
    string ModuleId { get; }

    IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels();

    IReadOnlyCollection<ReadModelRecord> Project(string readModelKey, ReadModelProjectionRequest request);
}
