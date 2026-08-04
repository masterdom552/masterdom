using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Services;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Handlers.Commands;

public sealed class StartOptimizationCommandHandler : ICommandHandler<StartOptimizationCommand, ExecutionResult<OptimizationRunAggregate>>
{
    private readonly ISubsidyOptimizationApplicationService _applicationService;

    public StartOptimizationCommandHandler(ISubsidyOptimizationApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<OptimizationRunAggregate> Handle(StartOptimizationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var run = _applicationService.StartOptimization(command);
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
