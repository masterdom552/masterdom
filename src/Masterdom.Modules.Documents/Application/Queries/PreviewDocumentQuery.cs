namespace Masterdom.Modules.Documents.Application.Queries;

public sealed record PreviewDocumentQuery(
    string DocumentType,
    Guid RequestedBy,
    IReadOnlyDictionary<string, string> Parameters,
    string? TemplateCode,
    int? TemplateVersion);
