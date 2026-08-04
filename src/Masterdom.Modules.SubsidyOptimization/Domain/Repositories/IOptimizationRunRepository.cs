using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Repositories;

public interface IOptimizationRunRepository
{
    void Add(OptimizationRunAggregate optimizationRun);

    void Update(OptimizationRunAggregate optimizationRun);

    OptimizationRunAggregate? GetById(OptimizationRunId id);

    OptimizationRunAggregate? GetByScenarioPeriodAndVersion(
        ScenarioId scenarioId,
        OptimizationPeriod optimizationPeriod,
        OptimizationVersion optimizationVersion);

    OptimizationRunAggregate? GetLatestByScenarioAndPeriod(
        ScenarioId scenarioId,
        OptimizationPeriod optimizationPeriod);
}
