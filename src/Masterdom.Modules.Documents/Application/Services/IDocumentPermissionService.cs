namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentPermissionService
{
    void EnsureCanGenerate(string documentType);

    void EnsureCanDownload(string documentId);
}
