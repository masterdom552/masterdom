namespace Masterdom.Modules.Reporting.Application.Models;

public sealed record ReportTemplate(
    string Name,
    string ReportCode,
    string SortBy,
    bool SortDescending,
    int PageSize,
    IReadOnlyDictionary<string, string> Filters);
