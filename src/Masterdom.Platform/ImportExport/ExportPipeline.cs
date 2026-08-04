namespace Masterdom.Platform.ImportExport;

public sealed class ExportPipeline : IExportPipeline
{
    private readonly IImportExportRegistry _registry;

    public ExportPipeline(IImportExportRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public ExportResult Execute(ExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var provider = _registry.ResolveExportProvider(request.Format);
        return provider.WriteRows(request);
    }
}
