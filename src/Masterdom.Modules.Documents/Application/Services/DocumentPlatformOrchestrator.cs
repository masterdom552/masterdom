using Masterdom.Modules.Documents.Application.Models;

namespace Masterdom.Modules.Documents.Application.Services;

public sealed class DocumentPlatformOrchestrator : IDocumentPlatformOrchestrator
{
    public void OnDocumentGenerated(GeneratedDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
    }
}
