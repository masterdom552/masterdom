using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public interface IDocumentPlatformOrchestrator
{
    void OnDocumentGenerated(GeneratedDocument document);
}
