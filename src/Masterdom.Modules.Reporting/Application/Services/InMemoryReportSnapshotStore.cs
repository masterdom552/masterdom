using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

public sealed class InMemoryReportSnapshotStore : IReportSnapshotStore
{
    private readonly Dictionary<string, ReportSnapshot> _snapshots = new(StringComparer.OrdinalIgnoreCase);

    public ReportSnapshot Save(string reportCode, ReportDataSet dataSet, DateTime createdAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportCode);
        ArgumentNullException.ThrowIfNull(dataSet);

        var snapshot = new ReportSnapshot(
            Guid.CreateVersion7().ToString("N"),
            reportCode,
            createdAtUtc,
            dataSet);

        _snapshots[snapshot.SnapshotId] = snapshot;
        return snapshot;
    }
}
