using System.Text;
using Masterdom.Modules.Reporting.Application.Export;
using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

public sealed class ReportExportService : IReportExportService
{
    public (string MimeType, string FileName, string Content) Export(string reportCode, ReportExportFormat format, ReportDataSet dataSet)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportCode);
        ArgumentNullException.ThrowIfNull(dataSet);

        return format switch
        {
            ReportExportFormat.Csv => ("text/csv", $"{reportCode}.csv", BuildDelimited(dataSet, ',')),
            ReportExportFormat.Excel => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportCode}.xlsx", BuildDelimited(dataSet, '\t')),
            ReportExportFormat.Pdf => ("application/pdf", $"{reportCode}.pdf", BuildPdfLikeContent(dataSet)),
            _ => throw new InvalidOperationException("Unsupported export format.")
        };
    }

    private static string BuildDelimited(ReportDataSet dataSet, char delimiter)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(delimiter, dataSet.Columns.Select(x => x.Label)));

        foreach (var row in dataSet.Rows)
        {
            var values = dataSet.Columns.Select(column => row.Values.TryGetValue(column.Key, out var value) ? value : string.Empty);
            builder.AppendLine(string.Join(delimiter, values));
        }

        return builder.ToString();
    }

    private static string BuildPdfLikeContent(ReportDataSet dataSet)
    {
        var builder = new StringBuilder();
        builder.AppendLine("MASTERDOM REPORT");
        builder.AppendLine(string.Join(" | ", dataSet.Columns.Select(x => x.Label)));

        foreach (var row in dataSet.Rows)
        {
            var values = dataSet.Columns.Select(column => row.Values.TryGetValue(column.Key, out var value) ? value : string.Empty);
            builder.AppendLine(string.Join(" | ", values));
        }

        return builder.ToString();
    }
}
