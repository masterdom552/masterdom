using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

public interface IReportTemplateStore
{
    void Save(ReportTemplate template);

    ReportTemplate? Get(string reportCode, string templateName);
}
