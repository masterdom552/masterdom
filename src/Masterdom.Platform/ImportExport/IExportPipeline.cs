namespace Masterdom.Platform.ImportExport;

public interface IExportPipeline
{
    ExportResult Execute(ExportRequest request);
}
