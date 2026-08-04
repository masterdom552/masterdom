namespace Masterdom.Platform.ReadModels;

public sealed record ReadModelMetadata(
    string ModuleId,
    string ReadModelKey,
    int Version,
    string Description,
    string Provider,
    IReadOnlyCollection<string> ConsumerCompatibility,
    IReadOnlyCollection<string> SupportedParameters,
    IReadOnlyDictionary<string, string> OutputSchema);

public sealed record ReportReadModelRegistration(
    string ReportCode,
    IReadOnlyCollection<string> ReadModelKeys,
    IReadOnlyCollection<string> SupportedParameters,
    IReadOnlyDictionary<string, string> OutputSchema,
    string Description);
