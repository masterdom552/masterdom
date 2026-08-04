namespace Masterdom.Platform.ImportExport;

public interface IImportProvider
{
    ImportExportFormat Format { get; }

    IReadOnlyCollection<IReadOnlyDictionary<string, string>> ReadRows(
        Stream content,
    ImportDefinition definition);
}
