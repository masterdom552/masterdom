namespace Masterdom.Modules.Documents.Application.Models;

public sealed record DocumentHistoryEntry(
    string DocumentId,
    string DocumentType,
    DocumentCategory Category,
    string TemplateCode,
    int TemplateVersion,
    string FileName,
    string MimeType,
    string Content,
    DateTime GeneratedAtUtc,
    Guid RequestedBy,
    IReadOnlyDictionary<string, string> Parameters);
