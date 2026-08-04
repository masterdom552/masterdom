using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Documents.Application.Services;

public sealed class DocumentApplicationService : IDocumentApplicationService
{
    private readonly IDocumentReadModelRegistry _readModelRegistry;
    private readonly IDocumentTemplateStore _templateStore;
    private readonly IDocumentHistoryStore _historyStore;
    private readonly IDocumentPermissionService _permissionService;
    private readonly IDocumentRenderer _renderer;
    private readonly IReadModelProjectionOrchestrator _projectionOrchestrator;
    private readonly IDocumentPlatformOrchestrator _platformOrchestrator;

    public DocumentApplicationService(
        IDocumentReadModelRegistry readModelRegistry,
        IDocumentTemplateStore templateStore,
        IDocumentHistoryStore historyStore,
        IDocumentPermissionService permissionService,
        IDocumentRenderer renderer,
        IReadModelProjectionOrchestrator projectionOrchestrator,
        IDocumentPlatformOrchestrator platformOrchestrator)
    {
        _readModelRegistry = readModelRegistry ?? throw new ArgumentNullException(nameof(readModelRegistry));
        _templateStore = templateStore ?? throw new ArgumentNullException(nameof(templateStore));
        _historyStore = historyStore ?? throw new ArgumentNullException(nameof(historyStore));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _projectionOrchestrator = projectionOrchestrator ?? throw new ArgumentNullException(nameof(projectionOrchestrator));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public GeneratedDocument Generate(
        string documentType,
        Guid requestedBy,
        IReadOnlyDictionary<string, string> parameters,
        string? templateCode,
        int? templateVersion,
        DocumentExportFormat exportFormat)
    {
        var normalizedType = DocumentTypeCatalog.Normalize(documentType);
        _permissionService.EnsureCanGenerate(normalizedType);

        var registration = _readModelRegistry.Resolve(normalizedType);
        var template = _templateStore.Resolve(normalizedType, templateCode ?? registration.DefaultTemplateCode, templateVersion);

        var hydrated = HydrateParameters(registration.ReadModelKey, parameters);
        var content = _renderer.Render(template.Layout, hydrated);

        var document = new GeneratedDocument(
            Guid.CreateVersion7().ToString("N"),
            normalizedType,
            registration.Category,
            template.TemplateCode,
            template.Version,
            BuildFileName(normalizedType, exportFormat),
            ResolveMimeType(exportFormat),
            content,
            BuildPreview(content),
            DateTime.UtcNow,
            hydrated);

        _historyStore.Save(new DocumentHistoryEntry(
            document.DocumentId,
            document.DocumentType,
            document.Category,
            document.TemplateCode,
            document.TemplateVersion,
            document.FileName,
            document.MimeType,
            document.Content,
            document.GeneratedAtUtc,
            requestedBy,
            document.Parameters));

        _platformOrchestrator.OnDocumentGenerated(document);

        return document;
    }

    public GeneratedDocument Preview(
        string documentType,
        Guid requestedBy,
        IReadOnlyDictionary<string, string> parameters,
        string? templateCode,
        int? templateVersion)
    {
        var generated = Generate(
            documentType,
            requestedBy,
            parameters,
            templateCode,
            templateVersion,
            DocumentExportFormat.Html);

        return generated with
        {
            MimeType = "text/html",
            FileName = BuildFileName(generated.DocumentType, DocumentExportFormat.Html)
        };
    }

    public GeneratedDocument Download(string documentId)
    {
        _permissionService.EnsureCanDownload(documentId);

        var history = _historyStore.GetById(documentId)
            ?? throw new InvalidOperationException($"No document found for id '{documentId}'.");

        return new GeneratedDocument(
            history.DocumentId,
            history.DocumentType,
            history.Category,
            history.TemplateCode,
            history.TemplateVersion,
            history.FileName,
            history.MimeType,
            history.Content,
            BuildPreview(history.Content),
            history.GeneratedAtUtc,
            history.Parameters);
    }

    public GeneratedDocument Regenerate(string documentId, Guid requestedBy, DocumentExportFormat exportFormat)
    {
        var history = _historyStore.GetById(documentId)
            ?? throw new InvalidOperationException($"No document found for id '{documentId}'.");

        return Generate(
            history.DocumentType,
            requestedBy,
            history.Parameters,
            history.TemplateCode,
            history.TemplateVersion,
            exportFormat);
    }

    public IReadOnlyCollection<DocumentHistoryEntry> History(string documentType, int page, int pageSize)
    {
        var normalizedType = DocumentTypeCatalog.Normalize(documentType);
        return _historyStore.GetByDocumentType(normalizedType, page, pageSize);
    }

    private IReadOnlyDictionary<string, string> HydrateParameters(
        string readModelKey,
        IReadOnlyDictionary<string, string> parameters)
    {
        var hydrated = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase);

        var projections = _projectionOrchestrator.Project(readModelKey, new ReadModelProjectionRequest(parameters, DateTime.UtcNow));
        var firstRecord = projections.SelectMany(x => x.Records).FirstOrDefault();
        if (firstRecord is null)
        {
            return hydrated;
        }

        foreach (var kv in firstRecord.Fields)
        {
            if (!hydrated.ContainsKey(kv.Key))
            {
                hydrated[kv.Key] = kv.Value;
            }
        }

        return hydrated;
    }

    private static string BuildPreview(string content)
    {
        return content.Length <= 320 ? content : content[..320];
    }

    private static string ResolveMimeType(DocumentExportFormat exportFormat)
    {
        return exportFormat switch
        {
            DocumentExportFormat.Pdf => "application/pdf",
            DocumentExportFormat.Html => "text/html",
            DocumentExportFormat.Text => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static string BuildFileName(string documentType, DocumentExportFormat exportFormat)
    {
        var extension = exportFormat switch
        {
            DocumentExportFormat.Pdf => "pdf",
            DocumentExportFormat.Html => "html",
            DocumentExportFormat.Text => "txt",
            _ => "dat"
        };

        return $"{documentType}-{DateTime.UtcNow:yyyyMMddHHmmss}.{extension}";
    }
}
