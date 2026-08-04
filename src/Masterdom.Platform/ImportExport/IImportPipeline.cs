namespace Masterdom.Platform.ImportExport;

public interface IImportPipeline
{
    ImportResult Execute(ImportRequest request);
}
