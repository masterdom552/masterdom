using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Support;
using Masterdom.Platform.ReadModels;

namespace Masterdom.Modules.Reporting.Application.Services;

public sealed class ReportApplicationService : IReportApplicationService
{
    private readonly IReportReadModelRegistry _reportReadModelRegistry;
    private readonly IReadModelRegistry _readModelRegistry;
    private readonly IReadModelProjectionOrchestrator _projectionOrchestrator;
    private readonly IReportTemplateStore _templateStore;
    private readonly IReportSnapshotStore _snapshotStore;
    private readonly IReportPermissionService _permissionService;
    private readonly IReportExportService _exportService;
    private readonly IReportPlatformOrchestrator _platformOrchestrator;

    public ReportApplicationService(
        IReportReadModelRegistry reportReadModelRegistry,
        IReadModelRegistry readModelRegistry,
        IReadModelProjectionOrchestrator projectionOrchestrator,
        IReportTemplateStore templateStore,
        IReportSnapshotStore snapshotStore,
        IReportPermissionService permissionService,
        IReportExportService exportService,
        IReportPlatformOrchestrator platformOrchestrator)
    {
        _reportReadModelRegistry = reportReadModelRegistry ?? throw new ArgumentNullException(nameof(reportReadModelRegistry));
        _readModelRegistry = readModelRegistry ?? throw new ArgumentNullException(nameof(readModelRegistry));
        _projectionOrchestrator = projectionOrchestrator ?? throw new ArgumentNullException(nameof(projectionOrchestrator));
        _templateStore = templateStore ?? throw new ArgumentNullException(nameof(templateStore));
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public GeneratedReport Generate(GenerateReportQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var normalizedCode = query.ReportCode.Trim().ToLowerInvariant();
        if (!ReportCatalog.Codes.Contains(normalizedCode, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Unsupported report code '{query.ReportCode}'.", nameof(query));
        }

        if (!_permissionService.HasPermission(normalizedCode))
        {
            throw new InvalidOperationException("The current user does not have permission to access this report.");
        }

        if (!ReportColumns.Map.TryGetValue(normalizedCode, out var columns))
        {
            throw new InvalidOperationException($"No report schema is configured for '{normalizedCode}'.");
        }

        var template = ResolveTemplate(query, normalizedCode);
        var request = BuildRequest(query, normalizedCode, template);

        var registration = _reportReadModelRegistry.Resolve(normalizedCode);
        var rows = ResolveRowsFromReadModels(registration, request.Filters);
        var sorted = SortRows(rows, request.SortBy, request.SortDescending);
        var paged = ApplyPaging(sorted, request.Page, request.PageSize);

        var dataSet = new ReportDataSet(
            columns,
            paged,
            sorted.Count,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDescending);

        var snapshot = request.CreateSnapshot
            ? _snapshotStore.Save(normalizedCode, dataSet, DateTime.UtcNow)
            : null;

        var export = _exportService.Export(normalizedCode, request.ExportFormat, dataSet);

        var kpis = BuildKpis(normalizedCode, dataSet);
        var dashboard = BuildDashboard(normalizedCode, dataSet);

        ReportTemplate? appliedTemplate = template;

        if (!string.IsNullOrWhiteSpace(query.TemplateName))
        {
            var savedTemplate = new ReportTemplate(
                query.TemplateName,
                normalizedCode,
                request.SortBy,
                request.SortDescending,
                request.PageSize,
                request.Filters);

            _templateStore.Save(savedTemplate);
            appliedTemplate = savedTemplate;
        }

        _platformOrchestrator.OnReportGenerated(normalizedCode, DateTime.UtcNow);

        return new GeneratedReport(
            normalizedCode,
            export.MimeType,
            export.FileName,
            export.Content,
            dataSet,
            snapshot,
            appliedTemplate,
            kpis,
            dashboard);
    }

    private IReadOnlyCollection<ReportRow> ResolveRowsFromReadModels(
        ReportReadModelRegistration registration,
        IReadOnlyDictionary<string, string> filters)
    {
        var request = new ReadModelProjectionRequest(filters, DateTime.UtcNow);

        var records = new List<ReadModelRecord>();
        foreach (var key in registration.ReadModelKeys)
        {
            var metadata = _readModelRegistry.ResolveMetadata(key);
            if (metadata.Count == 0)
            {
                throw new InvalidOperationException($"No metadata is registered for read model key '{key}'.");
            }

            var projections = _projectionOrchestrator.Project(key, request);
            records.AddRange(projections.SelectMany(x => x.Records));
        }

        return records.Select(x => new ReportRow(x.Fields)).ToList();
    }

    private ReportTemplate? ResolveTemplate(GenerateReportQuery query, string reportCode)
    {
        if (string.IsNullOrWhiteSpace(query.TemplateName))
        {
            return null;
        }

        return _templateStore.Get(reportCode, query.TemplateName);
    }

    private static ReportRequest BuildRequest(GenerateReportQuery query, string reportCode, ReportTemplate? template)
    {
        var sortBy = string.IsNullOrWhiteSpace(query.SortBy)
            ? template?.SortBy ?? "id"
            : query.SortBy.Trim();

        var sortDescending = template?.SortDescending ?? query.SortDescending;
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var filters = template?.Filters ?? query.Filters;

        return new ReportRequest(
            reportCode,
            sortBy,
            sortDescending,
            page,
            pageSize,
            query.ExportFormat,
            query.TemplateName,
            query.CreateSnapshot,
            filters);
    }

    private static IReadOnlyCollection<ReportRow> SortRows(IReadOnlyCollection<ReportRow> rows, string sortBy, bool sortDescending)
    {
        var ordered = sortDescending
            ? rows.OrderByDescending(x => x.Values.TryGetValue(sortBy, out var value) ? value : string.Empty, StringComparer.OrdinalIgnoreCase)
            : rows.OrderBy(x => x.Values.TryGetValue(sortBy, out var value) ? value : string.Empty, StringComparer.OrdinalIgnoreCase);

        return ordered.ToList();
    }

    private static IReadOnlyCollection<ReportRow> ApplyPaging(IReadOnlyCollection<ReportRow> rows, int page, int pageSize)
    {
        return rows.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    private static IReadOnlyCollection<string> BuildKpis(string reportCode, ReportDataSet dataSet)
    {
        return
        [
            $"Report: {reportCode}",
            $"Rows: {dataSet.TotalCount}",
            $"Page: {dataSet.Page}/{Math.Max(1, (int)Math.Ceiling(dataSet.TotalCount / (decimal)dataSet.PageSize))}"
        ];
    }

    private static IReadOnlyCollection<string> BuildDashboard(string reportCode, ReportDataSet dataSet)
    {
        return
        [
            $"Dashboard {reportCode}",
            $"Visible Rows: {dataSet.Rows.Count}",
            $"Sort: {dataSet.SortBy} {(dataSet.SortDescending ? "desc" : "asc")}"
        ];
    }
}
