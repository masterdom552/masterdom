using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentReadModelRegistry
{
    DocumentReadModelRegistration Resolve(string documentType);

    IReadOnlyCollection<DocumentReadModelRegistration> GetAll();
}
