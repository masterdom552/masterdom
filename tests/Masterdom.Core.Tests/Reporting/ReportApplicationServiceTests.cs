using Masterdom.Modules.Reporting.Application.Export;
using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Services;
using Masterdom.Modules.Reporting.Application.Support;
using Masterdom.Modules.Reporting.Application.Handlers.Queries;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Core.Tests.Reporting;

public sealed class ReportApplicationServiceTests
{
    [Fact]
    public void Generate_ShouldReturnExportDataSetSnapshotAndTemplateForMonthlyDashboard()
    {
        var orchestrator = new FakeProjectionOrchestrator();
        var templates = new InMemoryReportTemplateStore();
        var snapshots = new InMemoryReportSnapshotStore();
        var service = new ReportApplicationService(
            new ReportReadModelRegistry(),
            new FakeReadModelRegistry(),
            orchestrator,
            templates,
            snapshots,
            new AllowAllPermissionService(),
            new ReportExportService(),
            new NoopOrchestrator());

        var query = new GenerateReportQuery(
            "monthly-dashboard",
            "period",
            false,
            1,
            20,
            ReportExportFormat.Csv,
            "dashboard-template",
            true,
            new Dictionary<string, string>());

        var result = service.Generate(query);

        Assert.Equal("monthly-dashboard", result.ReportCode);
        Assert.Equal("text/csv", result.MimeType);
        Assert.NotNull(result.Snapshot);
        Assert.NotNull(result.AppliedTemplate);
        Assert.NotEmpty(result.ExportContent);
        Assert.NotEmpty(result.Kpis);
        Assert.NotEmpty(result.DashboardSummaries);
    }

    [Fact]
    public void Generate_ShouldThrow_WhenPermissionDenied()
    {
        var service = new ReportApplicationService(
            new ReportReadModelRegistry(),
            new FakeReadModelRegistry(),
            new FakeProjectionOrchestrator(),
            new InMemoryReportTemplateStore(),
            new InMemoryReportSnapshotStore(),
            new DenyPermissionService(),
            new ReportExportService(),
            new NoopOrchestrator());

        Assert.Throws<InvalidOperationException>(() =>
            service.Generate(new GenerateReportQuery(
                "active-tenancies",
                "tenancyId",
                false,
                1,
                10,
                ReportExportFormat.Pdf,
                null,
                false,
                new Dictionary<string, string>())));
    }

    [Fact]
    public void QueryHandler_ShouldReturnFailure_WhenUnsupportedReportRequested()
    {
        var service = new ReportApplicationService(
            new ReportReadModelRegistry(),
            new FakeReadModelRegistry(),
            new FakeProjectionOrchestrator(),
            new InMemoryReportTemplateStore(),
            new InMemoryReportSnapshotStore(),
            new AllowAllPermissionService(),
            new ReportExportService(),
            new NoopOrchestrator());

        var handler = new GenerateReportQueryHandler(service);

        var result = handler.Handle(new GenerateReportQuery(
            "unsupported",
            "id",
            false,
            1,
            10,
            ReportExportFormat.Excel,
            null,
            false,
            new Dictionary<string, string>()));

        Assert.False(result.IsSuccess);
        Assert.Equal("validation_failed", result.ErrorCode);
    }

    private sealed class FakeProjectionOrchestrator : IReadModelProjectionOrchestrator
    {
        public IReadOnlyCollection<ReadModelProjectionResult> Project(string readModelKey, ReadModelProjectionRequest request)
        {
            _ = readModelKey;
            _ = request;

            return
            [
                new ReadModelProjectionResult(
                    new ReadModelMetadata(
                        "fake",
                        BaselineReadModelKeys.OccupancySummary,
                        1,
                        "fake",
                        "FakeProjectionOrchestrator",
                        ["Reporting"],
                        [],
                        new Dictionary<string, string> { ["period"] = "string" }),
                    [
                        new ReadModelRecord(new Dictionary<string, string>
                        {
                            ["period"] = "2026-08",
                            ["occupancyRate"] = "91.20",
                            ["collections"] = "50000",
                            ["outstanding"] = "4200"
                        }),
                        new ReadModelRecord(new Dictionary<string, string>
                        {
                            ["period"] = "2026-07",
                            ["occupancyRate"] = "90.50",
                            ["collections"] = "49000",
                            ["outstanding"] = "4500"
                        })
                    ],
                    DateTime.UtcNow)
            ];
        }
    }

    private sealed class AllowAllPermissionService : IReportPermissionService
    {
        public bool HasPermission(string reportCode) => true;
    }

    private sealed class DenyPermissionService : IReportPermissionService
    {
        public bool HasPermission(string reportCode) => false;
    }

    private sealed class NoopOrchestrator : IReportPlatformOrchestrator
    {
        public void OnReportGenerated(string reportCode, DateTime generatedAtUtc)
        {
        }
    }

    private sealed class FakeReadModelRegistry : IReadModelRegistry
    {
        public IReadOnlyCollection<IReadModelProvider> GetProviders() => [];

        public IReadOnlyCollection<ReadModelMetadata> GetRegisteredReadModels() =>
        [
            new(
                "fake",
                BaselineReadModelKeys.OccupancySummary,
                1,
                "fake",
                "Fake",
                ["Reporting"],
                [],
                new Dictionary<string, string> { ["period"] = "string" }),
            new(
                "fake",
                BaselineReadModelKeys.CollectionSummary,
                1,
                "fake",
                "Fake",
                ["Reporting"],
                [],
                new Dictionary<string, string> { ["totalCollections"] = "string" }),
            new(
                "fake",
                BaselineReadModelKeys.BillingSummary,
                1,
                "fake",
                "Fake",
                ["Reporting"],
                [],
                new Dictionary<string, string> { ["totalOutstanding"] = "string" })
        ];

        public IReadOnlyCollection<IReadModelProvider> ResolveProviders(string readModelKey) => [];

        public IReadOnlyCollection<ReadModelMetadata> ResolveMetadata(string readModelKey)
        {
            return GetRegisteredReadModels()
                .Where(x => x.ReadModelKey.Equals(readModelKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
