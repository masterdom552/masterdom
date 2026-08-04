using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Services;

public sealed class InMemoryReportTemplateStore : IReportTemplateStore
{
    private readonly Dictionary<string, ReportTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);

    public void Save(ReportTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        _templates[$"{template.ReportCode}:{template.Name}".ToUpperInvariant()] = template;
    }

    public ReportTemplate? Get(string reportCode, string templateName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reportCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);

        _templates.TryGetValue($"{reportCode}:{templateName}".ToUpperInvariant(), out var template);
        return template;
    }
}
