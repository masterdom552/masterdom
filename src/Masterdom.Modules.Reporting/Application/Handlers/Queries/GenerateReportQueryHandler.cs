using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Services;
using Masterdom.Modules.Reporting.Application.Support;
using Masterdom.Modules.Reporting.Application.Models;

namespace Masterdom.Modules.Reporting.Application.Handlers.Queries;

public sealed class GenerateReportQueryHandler : IQueryHandler<GenerateReportQuery, ExecutionResult<GeneratedReport>>
{
    private readonly IReportApplicationService _applicationService;

    public GenerateReportQueryHandler(IReportApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<GeneratedReport> Handle(GenerateReportQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var report = _applicationService.Generate(query);
            return ExecutionResult<GeneratedReport>.Success(report);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<GeneratedReport>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<GeneratedReport>.Failure("not_allowed", ex.Message);
        }
    }
}
