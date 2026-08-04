using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentHistoryStore
{
    void Save(DocumentHistoryEntry entry);

    DocumentHistoryEntry? GetById(string documentId);

    IReadOnlyCollection<DocumentHistoryEntry> GetByDocumentType(string documentType, int page, int pageSize);
}
