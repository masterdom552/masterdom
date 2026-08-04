namespace Masterdom.Platform.ImportExport;

public sealed class ImportExportRegistry : IImportExportRegistry
{
    private readonly IReadOnlyDictionary<ImportExportFormat, IImportProvider> _importProviders;
    private readonly IReadOnlyDictionary<ImportExportFormat, IExportProvider> _exportProviders;

    public ImportExportRegistry(
        IEnumerable<IImportProvider> importProviders,
        IEnumerable<IExportProvider> exportProviders)
    {
        _importProviders = importProviders?.ToDictionary(x => x.Format)
            ?? throw new ArgumentNullException(nameof(importProviders));

        _exportProviders = exportProviders?.ToDictionary(x => x.Format)
            ?? throw new ArgumentNullException(nameof(exportProviders));
    }

    public IImportProvider ResolveImportProvider(ImportExportFormat format)
    {
        return _importProviders.TryGetValue(format, out var provider)
            ? provider
            : throw new InvalidOperationException($"No import provider registered for format '{format}'.");
    }

    public IExportProvider ResolveExportProvider(ImportExportFormat format)
    {
        return _exportProviders.TryGetValue(format, out var provider)
            ? provider
            : throw new InvalidOperationException($"No export provider registered for format '{format}'.");
    }
}
