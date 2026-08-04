namespace Masterdom.Platform.ImportExport;

public sealed record ExportResult(
    string MimeType,
    string FileName,
    byte[] Content);
