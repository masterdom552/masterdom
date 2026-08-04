namespace Masterdom.Modules.Reporting.Application.Support;

public interface IReportPlatformOrchestrator
{
    void OnReportGenerated(string reportCode, DateTime generatedAtUtc);
}
