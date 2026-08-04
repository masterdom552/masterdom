using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentTemplateStore
{
    DocumentTemplate Resolve(string documentType, string? templateCode, int? version);

    IReadOnlyCollection<DocumentTemplate> GetByDocumentType(string documentType);
}
