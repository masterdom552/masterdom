namespace Masterdom.Platform.ImportExport;

public sealed record ImportProgress(
    int TotalRows,
    int ProcessedRows,
    int SuccessfulRows,
    int FailedRows);
