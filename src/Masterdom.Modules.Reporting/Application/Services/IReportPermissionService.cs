namespace Masterdom.Modules.Reporting.Application.Services;

public interface IReportPermissionService
{
    bool HasPermission(string reportCode);
}
