namespace Masterdom.Platform.ReadModels;

public interface IReportReadModelRegistry
{
    IReadOnlyCollection<ReportReadModelRegistration> GetRegistrations();

    ReportReadModelRegistration Resolve(string reportCode);
}
