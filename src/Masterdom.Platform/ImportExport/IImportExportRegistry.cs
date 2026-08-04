namespace Masterdom.Platform.ImportExport;

public interface IImportExportRegistry
{
    IImportProvider ResolveImportProvider(ImportExportFormat format);

    IExportProvider ResolveExportProvider(ImportExportFormat format);
}
