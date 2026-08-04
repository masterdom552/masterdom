using Masterdom.Core.Security;

namespace Masterdom.Modules.Reporting.Application.Services;

public sealed class ReportPermissionService : IReportPermissionService
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ReportPermissionService(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
    }

    public bool HasPermission(string reportCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportCode);

        var user = _currentUserAccessor.GetCurrentUser();
        if (!user.IsAuthenticated)
        {
            return false;
        }

        if (user.IsInRole(MasterdomRoles.SuperUser))
        {
            return true;
        }

        return user.HasPermission("reports.read") || user.HasPermission($"reports.{reportCode}.read");
    }
}
