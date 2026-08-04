using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Services;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Handlers.Queries;

public sealed class GetLatestOptimizationRunQueryHandler : IQueryHandler<GetLatestOptimizationRunQuery, ExecutionResult<OptimizationRunAggregate>>
{
    private readonly ISubsidyOptimizationApplicationService _applicationService;

    public GetLatestOptimizationRunQueryHandler(ISubsidyOptimizationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<OptimizationRunAggregate> Handle(GetLatestOptimizationRunQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var run = _applicationService.GetLatestOptimizationRun(query);

        return run is null
            ? ExecutionResult<OptimizationRunAggregate>.Failure("not_found", "Optimization run was not found.")
            : ExecutionResult<OptimizationRunAggregate>.Success(run);
    }
}
