using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Masterdom.Modules.SubsidyOptimization.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Infrastructure.Persistence.SubsidyOptimization;

public sealed class OptimizationRunRepository : IOptimizationRunRepository
{
    private readonly MasterdomDbContext _dbContext;

    public OptimizationRunRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(OptimizationRunAggregate optimizationRun)
    {
        ArgumentNullException.ThrowIfNull(optimizationRun);
        _dbContext.OptimizationRuns.Add(optimizationRun);
    }

    public void Update(OptimizationRunAggregate optimizationRun)
    {
        ArgumentNullException.ThrowIfNull(optimizationRun);
        _dbContext.OptimizationRuns.Update(optimizationRun);
    }

    public OptimizationRunAggregate? GetById(OptimizationRunId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.OptimizationRuns
            .Include(x => x.Recommendations)
            .Include(x => x.Snapshots)
            .Include(x => x.VersionHistory)
            .FirstOrDefault(x => x.Id == id);
    }

    public OptimizationRunAggregate? GetByScenarioPeriodAndVersion(
        ScenarioId scenarioId,
        OptimizationPeriod optimizationPeriod,
        OptimizationVersion optimizationVersion)
    {
        ArgumentNullException.ThrowIfNull(scenarioId);
        ArgumentNullException.ThrowIfNull(optimizationPeriod);
        ArgumentNullException.ThrowIfNull(optimizationVersion);

        return _dbContext.OptimizationRuns
            .AsEnumerable()
            .FirstOrDefault(x =>
                x.Scenario.ScenarioId == scenarioId &&
                x.OptimizationPeriod == optimizationPeriod &&
                x.OptimizationVersion == optimizationVersion);
    }

    public OptimizationRunAggregate? GetLatestByScenarioAndPeriod(
        ScenarioId scenarioId,
        OptimizationPeriod optimizationPeriod)
    {
        ArgumentNullException.ThrowIfNull(scenarioId);
        ArgumentNullException.ThrowIfNull(optimizationPeriod);

        return _dbContext.OptimizationRuns
            .AsEnumerable()
            .Where(x => x.Scenario.ScenarioId == scenarioId && x.OptimizationPeriod == optimizationPeriod)
            .OrderByDescending(x => x.OptimizationVersion.Value)
            .FirstOrDefault();
    }
}
