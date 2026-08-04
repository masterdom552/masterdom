namespace Masterdom.Platform.Configuration;

public sealed record BusinessConfigurationMetadata(
    string DefinitionId,
    string Name,
    int Version,
    BusinessConfigurationStatus Status,
    string Description,
    DateTime EffectiveFromUtc,
    DateTime? EffectiveToUtc,
    string CreatedBy,
    string ModifiedBy,
    DateTime CreatedAtUtc,
    DateTime ModifiedAtUtc,
    IReadOnlyDictionary<string, string> AuditMetadata);
