using Masterdom.Modules.Reporting.Application.Export;

namespace Masterdom.Modules.Reporting.Application.Queries;

public sealed record GenerateReportQuery(
    string ReportCode,
    string SortBy,
    bool SortDescending,
    int Page,
    int PageSize,
    ReportExportFormat ExportFormat,
    string? TemplateName,
    bool CreateSnapshot,
    IReadOnlyDictionary<string, string> Filters);
