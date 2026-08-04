using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Services;
using System.Text.Json;

namespace Masterdom.Infrastructure.Persistence.Documents;

internal sealed class PersistentDocumentHistoryStore : IDocumentHistoryStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private readonly object _sync = new();

    public PersistentDocumentHistoryStore()
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data", "documents");
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "history.v1.json");

        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    public void Save(DocumentHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_sync)
        {
            var entries = LoadAll().ToList();
            entries.Add(entry);
            Persist(entries);
        }
    }

    public DocumentHistoryEntry? GetById(string documentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        lock (_sync)
        {
            return LoadAll().FirstOrDefault(x => x.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyCollection<DocumentHistoryEntry> GetByDocumentType(string documentType, int page, int pageSize)
    {
        var normalizedType = DocumentTypeCatalog.Normalize(documentType);
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : pageSize;

        lock (_sync)
        {
            return LoadAll()
                .Where(x => x.DocumentType.Equals(normalizedType, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.GeneratedAtUtc)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToList();
        }
    }

    private IReadOnlyCollection<DocumentHistoryEntry> LoadAll()
    {
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<DocumentHistoryEntry>>(json, JsonOptions) ?? [];
    }

    private void Persist(IReadOnlyCollection<DocumentHistoryEntry> entries)
    {
        var json = JsonSerializer.Serialize(entries, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
