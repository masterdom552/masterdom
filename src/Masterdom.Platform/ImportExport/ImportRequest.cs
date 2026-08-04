namespace Masterdom.Platform.ImportExport;

public sealed record ImportRequest(
    string JobCode,
    ImportExportFormat Format,
    Stream Content,
    ImportDefinitionCatalogReference DefinitionReference,
    bool ContinueOnRecoverableErrors);
