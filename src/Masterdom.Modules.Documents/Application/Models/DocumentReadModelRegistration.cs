namespace Masterdom.Modules.Documents.Application.Models;

public sealed record DocumentReadModelRegistration(
    string DocumentType,
    string ReadModelKey,
    DocumentCategory Category,
    string DefaultTemplateCode,
    IReadOnlyCollection<string> SupportedParameters,
    string Description);
