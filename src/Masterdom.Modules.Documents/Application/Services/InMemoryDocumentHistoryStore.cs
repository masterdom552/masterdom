using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public sealed class InMemoryDocumentHistoryStore : IDocumentHistoryStore
{
    private readonly List<DocumentHistoryEntry> _entries = [];

    public void Save(DocumentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
    }

    public DocumentHistoryEntry? GetById(string documentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        return _entries.FirstOrDefault(x => x.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyCollection<DocumentHistoryEntry> GetByDocumentType(string documentType, int page, int pageSize)
    {
        var normalizedType = DocumentTypeCatalog.Normalize(documentType);
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : pageSize;

        return _entries
            .Where(x => x.DocumentType.Equals(normalizedType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.GeneratedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();
    }
}
