using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentApplicationService
{
    GeneratedDocument Generate(
        string documentType,
        Guid requestedBy,
        IReadOnlyDictionary<string, string> parameters,
        string? templateCode,
        int? templateVersion,
        DocumentExportFormat exportFormat);

    GeneratedDocument Preview(
        string documentType,
        Guid requestedBy,
        IReadOnlyDictionary<string, string> parameters,
        string? templateCode,
        int? templateVersion);

    GeneratedDocument Download(string documentId);

    GeneratedDocument Regenerate(string documentId, Guid requestedBy, DocumentExportFormat exportFormat);

    IReadOnlyCollection<DocumentHistoryEntry> History(string documentType, int page, int pageSize);
}
