namespace Masterdom.Platform.ImportExport;

public interface IExportProvider
{
    ImportExportFormat Format { get; }

    ExportResult WriteRows(ExportRequest request);
}
