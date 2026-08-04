using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Masterdom.Modules.SubsidyOptimization.Domain.Repositories;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Services;

public sealed class SubsidyOptimizationApplicationService : ISubsidyOptimizationApplicationService
{
    private readonly IOptimizationRunRepository _repository;
    private readonly ISubsidyOptimizationUnitOfWork _unitOfWork;
    private readonly ISubsidyOptimizationPlatformOrchestrator _platformOrchestrator;

    public SubsidyOptimizationApplicationService(
        IOptimizationRunRepository repository,
        ISubsidyOptimizationUnitOfWork unitOfWork,
        ISubsidyOptimizationPlatformOrchestrator platformOrchestrator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
    }

    public OptimizationRunAggregate StartOptimization(StartOptimizationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = _repository.GetByScenarioPeriodAndVersion(
            command.Scenario.ScenarioId,
            command.OptimizationPeriod,
            OptimizationVersion.Initial);

        if (existing is not null)
        {
            throw new InvalidOperationException("An optimization run already exists for scenario, period, and version 1.");
        }

        var run = OptimizationRunAggregate.Start(
            OptimizationRunId.New(),
            command.Scenario,
            command.MeterGroup,
            command.ToRatingReference(),
            command.OptimizationPeriod,
            DateTime.UtcNow);

        _unitOfWork.Execute(() => _repository.Add(run));
        _platformOrchestrator.OnOptimizationRunMutated(run, "StartOptimization");

        return run;
    }

    public OptimizationRunAggregate CompleteOptimization(CompleteOptimizationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var run = GetRequiredOptimizationRun(command.OptimizationRunId);

        run.Complete(
            command.OptimizationResult,
            command.ConsumptionForecast,
            command.ToRecommendationSet(),
            command.CompletedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(run));
        _platformOrchestrator.OnOptimizationRunMutated(run, "CompleteOptimization");

        return run;
    }

    public OptimizationRunAggregate CreateScenarioVersion(CreateScenarioVersionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var run = GetRequiredOptimizationRun(command.OptimizationRunId);
        var next = run.CreateScenarioVersion(command.StartedAtUtc, command.ToRatingReference());

        var duplicate = _repository.GetByScenarioPeriodAndVersion(
            next.Scenario.ScenarioId,
            next.OptimizationPeriod,
            next.OptimizationVersion);

        if (duplicate is not null)
        {
            throw new InvalidOperationException("An optimization run already exists for the next scenario version.");
        }

        _unitOfWork.Execute(() => _repository.Add(next));
        _platformOrchestrator.OnOptimizationRunMutated(next, "CreateScenarioVersion");

        return next;
    }

    public OptimizationRunAggregate ArchiveRecommendation(ArchiveRecommendationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var run = GetRequiredOptimizationRun(command.OptimizationRunId);
        run.ArchiveRecommendation(command.RecommendationId, command.Reason, command.ArchivedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(run));
        _platformOrchestrator.OnOptimizationRunMutated(run, "ArchiveRecommendation");

        return run;
    }

    public OptimizationRunAggregate? GetOptimizationRun(GetOptimizationRunByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetById(query.OptimizationRunId);
    }

    public OptimizationRunAggregate? GetLatestOptimizationRun(GetLatestOptimizationRunQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.GetLatestByScenarioAndPeriod(query.ScenarioId, query.OptimizationPeriod);
    }

    private OptimizationRunAggregate GetRequiredOptimizationRun(OptimizationRunId optimizationRunId)
    {
        var run = _repository.GetById(optimizationRunId);
        if (run is null)
        {
            throw new InvalidOperationException($"Optimization run '{optimizationRunId}' was not found.");
        }

        return run;
    }
}
