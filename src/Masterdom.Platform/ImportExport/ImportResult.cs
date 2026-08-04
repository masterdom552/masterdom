namespace Masterdom.Platform.ImportExport;

public sealed record ImportResult(
    IReadOnlyCollection<IReadOnlyDictionary<string, string>> Rows,
    IReadOnlyCollection<ImportError> Errors,
    ImportProgress Progress);
