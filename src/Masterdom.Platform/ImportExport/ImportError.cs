namespace Masterdom.Platform.ImportExport;

public sealed record ImportError(
    int RowNumber,
    string Column,
    string OffendingValue,
    string Message,
    ImportExportSeverity Severity,
    bool IsRecoverable);
