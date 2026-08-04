using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Commands;

public sealed record RegenerateDocumentCommand(
    string DocumentId,
    Guid RequestedBy,
    DocumentExportFormat ExportFormat);
