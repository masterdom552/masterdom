using Masterdom.Modules.Reporting.Application.Export;
using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

public interface IReportExportService
{
    (string MimeType, string FileName, string Content) Export(string reportCode, ReportExportFormat format, ReportDataSet dataSet);
}
