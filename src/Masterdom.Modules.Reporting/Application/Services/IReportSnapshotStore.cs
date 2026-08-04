using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

public interface IReportSnapshotStore
{
    ReportSnapshot Save(string reportCode, ReportDataSet dataSet, DateTime createdAtUtc);
}
