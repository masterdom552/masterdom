using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Services;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Handlers.Commands;

public sealed class ArchiveRecommendationCommandHandler : ICommandHandler<ArchiveRecommendationCommand, ExecutionResult<OptimizationRunAggregate>>
{
    private readonly ISubsidyOptimizationApplicationService _applicationService;

    public ArchiveRecommendationCommandHandler(ISubsidyOptimizationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<OptimizationRunAggregate> Handle(ArchiveRecommendationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var run = _applicationService.ArchiveRecommendation(command);
            return ExecutionResult<OptimizationRunAggregate>.Success(run);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<OptimizationRunAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<OptimizationRunAggregate>.Failure("conflict", ex.Message);
        }
    }
}
