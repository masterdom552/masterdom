using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Commands;

public sealed record GenerateDocumentCommand(
    string DocumentType,
    Guid RequestedBy,
    IReadOnlyDictionary<string, string> Parameters,
    string? TemplateCode,
    int? TemplateVersion,
    DocumentExportFormat ExportFormat);
