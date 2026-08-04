namespace Masterdom.Platform.ReadModels;

public sealed record ReadModelProjectionResult(
    ReadModelMetadata Metadata,
    IReadOnlyCollection<ReadModelRecord> Records,
    DateTime GeneratedAtUtc);
