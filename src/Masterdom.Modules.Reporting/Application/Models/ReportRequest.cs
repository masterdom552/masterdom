using Masterdom.Modules.Reporting.Application.Export;

namespace Masterdom.Modules.Reporting.Application.Models;

public sealed class ReportRequest
{
    public ReportRequest(
        string reportCode,
        string sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        ReportExportFormat exportFormat,
        string? templateName,
        bool createSnapshot,
        IReadOnlyDictionary<string, string> filters)
    {
        ReportCode = reportCode;
        SortBy = sortBy;
        SortDescending = sortDescending;
        Page = page;
        PageSize = pageSize;
        ExportFormat = exportFormat;
        TemplateName = templateName;
        CreateSnapshot = createSnapshot;
        Filters = filters;
    }

    public string ReportCode { get; }

    public string SortBy { get; }

    public bool SortDescending { get; }

    public int Page { get; }

    public int PageSize { get; }

    public ReportExportFormat ExportFormat { get; }

    public string? TemplateName { get; }

    public bool CreateSnapshot { get; }

    public IReadOnlyDictionary<string, string> Filters { get; }
}
