namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record ImportedDatasetReference(
    string DatasetId,
    string DatasetType,
    string SourceSystem,
    string Version,
    DateTime ImportedAtUtc);
