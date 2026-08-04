using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Support;

public interface ISubsidyOptimizationPlatformOrchestrator
{
    void OnOptimizationRunMutated(OptimizationRunAggregate optimizationRun, string operationName);
}
