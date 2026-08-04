using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;

namespace Masterdom.Modules.Reporting.Application.Services;

public interface IReportApplicationService
{
    GeneratedReport Generate(GenerateReportQuery query);
}
