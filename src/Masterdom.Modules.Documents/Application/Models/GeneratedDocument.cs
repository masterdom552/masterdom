namespace Masterdom.Modules.Documents.Application.Models;

public sealed record GeneratedDocument(
    string DocumentId,
    string DocumentType,
    DocumentCategory Category,
    string TemplateCode,
    int TemplateVersion,
    string FileName,
    string MimeType,
    string Content,
    string Preview,
    DateTime GeneratedAtUtc,
    IReadOnlyDictionary<string, string> Parameters);
