namespace Masterdom.Platform.ImportExport;

public sealed record ExportRequest(
    string JobCode,
    ImportExportFormat Format,
    ImportDefinition Definition,
    IReadOnlyCollection<IReadOnlyDictionary<string, string>> Rows);
