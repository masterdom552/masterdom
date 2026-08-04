namespace Masterdom.Modules.Reporting.Application.Models;

public sealed record GeneratedReport(
    string ReportCode,
    string MimeType,
    string ExportFileName,
    string ExportContent,
    ReportDataSet DataSet,
    ReportSnapshot? Snapshot,
    ReportTemplate? AppliedTemplate,
    IReadOnlyCollection<string> Kpis,
    IReadOnlyCollection<string> DashboardSummaries);
