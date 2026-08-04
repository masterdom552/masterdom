namespace Masterdom.Modules.Reporting.Application.Models;

public sealed record ReportSnapshot(
    string SnapshotId,
    string ReportCode,
    DateTime CreatedAtUtc,
    ReportDataSet DataSet);
