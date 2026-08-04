namespace Masterdom.Modules.Documents.Application.Models;

public sealed record DocumentTemplate(
    string TemplateCode,
    string DocumentType,
    int Version,
    bool IsActive,
    string Layout,
    IReadOnlyCollection<string> ParameterKeys,
    IReadOnlyDictionary<string, string> Metadata);
