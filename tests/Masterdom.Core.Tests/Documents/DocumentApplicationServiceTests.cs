using Masterdom.Modules.Documents.Application.Models;
using Masterdom.Modules.Documents.Application.Services;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Core.Tests.Documents;

public sealed class DocumentApplicationServiceTests
{
    [Fact]
    public void Generate_ShouldSupportEveryRequiredV1DocumentType()
    {
        var service = CreateService();

        foreach (var documentType in DocumentTypeCatalog.All)
        {
            var generated = service.Generate(
                documentType,
                Guid.NewGuid(),
                new Dictionary<string, string>(),
                null,
                null,
                DocumentExportFormat.Pdf);

            Assert.Equal(documentType, generated.DocumentType);
            Assert.NotEmpty(generated.DocumentId);
            Assert.NotEmpty(generated.Content);
            Assert.EndsWith(".pdf", generated.FileName, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void PreviewDownloadRegenerateHistory_ShouldWork()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var generated = service.Generate(DocumentTypeCatalog.Bill, userId, new Dictionary<string, string>(), null, null, DocumentExportFormat.Text);
        var preview = service.Preview(DocumentTypeCatalog.Bill, userId, new Dictionary<string, string>(), null, null);
        var downloaded = service.Download(generated.DocumentId);
        var regenerated = service.Regenerate(generated.DocumentId, userId, DocumentExportFormat.Pdf);
        var history = service.History(DocumentTypeCatalog.Bill, 1, 20);

        Assert.Equal("text/html", preview.MimeType);
        Assert.Equal(generated.DocumentId, downloaded.DocumentId);
        Assert.EndsWith(".pdf", regenerated.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(history);
    }

    private static DocumentApplicationService CreateService()
    {
        return new DocumentApplicationService(
            new MetadataDrivenDocumentReadModelRegistry(),
            new InMemoryDocumentTemplateStore(),
            new InMemoryDocumentHistoryStore(),
            new AllowAllDocumentPermissionService(),
            new TextDocumentRenderer(),
            new FakeReadModelProjectionOrchestrator(),
            new DocumentPlatformOrchestrator());
    }

    private sealed class AllowAllDocumentPermissionService : IDocumentPermissionService
    {
        public void EnsureCanGenerate(string documentType)
        {
            _ = documentType;
        }

        public void EnsureCanDownload(string documentId)
        {
            _ = documentId;
        }
    }

    private sealed class FakeReadModelProjectionOrchestrator : IReadModelProjectionOrchestrator
    {
        public IReadOnlyCollection<ReadModelProjectionResult> Project(string readModelKey, ReadModelProjectionRequest request)
        {
            _ = readModelKey;
            _ = request;

            return
            [
                new ReadModelProjectionResult(
                    new ReadModelMetadata(
                        "test",
                        "test",
                        1,
                        "test",
                        "Fake",
                        ["Documents"],
                        [],
                        new Dictionary<string, string> { ["id"] = "string" }),
                    [
                        new ReadModelRecord(new Dictionary<string, string>
                        {
                            ["tenancyId"] = Guid.NewGuid().ToString("N"),
                            ["status"] = "Active",
                            ["moveInDate"] = "2026-08-01",
                            ["moveOutDate"] = "2026-09-01",
                            ["propertyId"] = Guid.NewGuid().ToString("N"),
                            ["occupancyRate"] = "90.0",
                            ["billNumber"] = "B-100",
                            ["outstandingAmount"] = "1200",
                            ["paymentReference"] = "P-001",
                            ["amount"] = "500",
                            ["reversedAt"] = "2026-08-01T00:00:00Z",
                            ["accountCode"] = "1000",
                            ["accountName"] = "Cash",
                            ["balance"] = "10000",
                            ["journalNumber"] = "J-10",
                            ["debits"] = "100",
                            ["credits"] = "100",
                            ["chargeTotal"] = "100"
                        })
                    ],
                    DateTime.UtcNow)
            ];
        }
    }
}
