using Masterdom.Modules.Reporting.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Reporting;

internal sealed class ReportingPlatformOrchestrator : IReportPlatformOrchestrator
{
    public void OnReportGenerated(string reportCode, DateTime generatedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportCode);
        _ = generatedAtUtc;
    }
}
