using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using Masterdom.Modules.SubsidyOptimization.Domain.Repositories;
using Masterdom.Platform.Configuration;
using System.Text.Json;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Services;

public sealed class SubsidyOptimizationApplicationService : ISubsidyOptimizationApplicationService
{
    private readonly IOptimizationRunRepository _repository;
    private readonly ISubsidyOptimizationUnitOfWork _unitOfWork;
    private readonly ISubsidyOptimizationPlatformOrchestrator _platformOrchestrator;
    private readonly ISubsidyMaximizerService _maximizer;

    public SubsidyOptimizationApplicationService(
        IOptimizationRunRepository repository,
        ISubsidyOptimizationUnitOfWork unitOfWork,
        ISubsidyOptimizationPlatformOrchestrator platformOrchestrator,
        ISubsidyMaximizerService maximizer)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _platformOrchestrator = platformOrchestrator ?? throw new ArgumentNullException(nameof(platformOrchestrator));
        _maximizer = maximizer ?? throw new ArgumentNullException(nameof(maximizer));
    }

    public OptimizationRunAggregate ExecuteOptimization(ExecuteSubsidyOptimizationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Request.ConsumptionHistory.Count == 0)
        {
            throw new ArgumentException("At least one metering consumption input is required.", nameof(command));
        }

        var existing = _repository.GetByScenarioPeriodAndVersion(
            command.Scenario.ScenarioId,
            command.OptimizationPeriod,
            OptimizationVersion.Initial);
        if (existing is not null)
        {
            throw new InvalidOperationException("An optimization run already exists for scenario, period, and version 1.");
        }

        SubsidyMaximizerResult result;
        try
        {
            result = _maximizer.Execute(command.Request);
        }
        catch (Exception ex) when (ex is PlatformConfigurationValidationException or JsonException)
        {
            throw new ArgumentException("Governed subsidy optimizer configuration is missing or invalid.", nameof(command), ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentException(ex.Message, nameof(command), ex);
        }
        var selected = result.RankedScenarios.FirstOrDefault(x => x.IsFeasible)
            ?? throw new InvalidOperationException("No feasible subsidy optimization scenario was produced.");
        var nowUtc = DateTime.UtcNow;
        var run = OptimizationRunAggregate.Start(
            OptimizationRunId.New(),
            command.Scenario,
            command.MeterGroup,
            RatingReference.Create(command.Request.RatedConsumptions.Select(x => x.RatingId).Distinct().ToArray()),
            command.OptimizationPeriod,
            nowUtc);

        var optimizationResult = OptimizationResult.Create(selected.ExpectedSubsidy, selected.ExpectedCost, selected.TradeOffSummary);
        var consumptionForecast = ConsumptionForecast.Create(
            result.ConsumptionEstimate.OccupancyAdjustedUnits,
            selected.ForecastConsumptionUnits,
            $"Configured strategy {result.GovernedConfiguration.Strategy.StrategyCode}");
        var recommendation = OptimizationRecommendation.Generate(
            RecommendationId.New(),
            $"Select {selected.ScenarioName}",
            $"{selected.TradeOffSummary};policy={result.GovernedConfiguration.Policy.PolicyCode}:v{result.GovernedConfiguration.PolicyIdentity.Version};model={result.GovernedConfiguration.Model.ModelCode}:v{result.GovernedConfiguration.ModelIdentity.Version};strategy={result.GovernedConfiguration.Strategy.StrategyCode}:v{result.GovernedConfiguration.StrategyIdentity.Version};boundary={selected.TriggeredBoundary?.ToString() ?? "none"}",
            RecommendationPriority.High,
            nowUtc);
        var evidence = CreateExecutionEvidence(command.Request, result, selected, optimizationResult, recommendation);

        run.Complete(
            optimizationResult,
            consumptionForecast,
            RecommendationSet.Create([recommendation]),
            nowUtc,
            evidence);

        _unitOfWork.Execute(() => _repository.Add(run));
        _platformOrchestrator.OnOptimizationRunMutated(run, "ExecuteOptimization");
        return run;
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

    public OptimizationRunAggregate ArchiveOptimizationRun(ArchiveOptimizationRunCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var run = GetRequiredOptimizationRun(command.OptimizationRunId);
        run.ArchiveRun(command.ArchivedAtUtc);

        _unitOfWork.Execute(() => _repository.Update(run));
        _platformOrchestrator.OnOptimizationRunMutated(run, "ArchiveOptimizationRun");
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

    private static OptimizationExecutionEvidence CreateExecutionEvidence(
        SubsidyMaximizerRequest request,
        SubsidyMaximizerResult result,
        SubsidyOptimizationScenario selected,
        OptimizationResult optimizationResult,
        OptimizationRecommendation recommendation)
    {
        static OptimizationConfigurationIdentity MapIdentity(ResolvedConfigurationIdentity identity) => new(
            identity.ConfigurationKey,
            identity.DefinitionId,
            identity.Version,
            identity.EffectiveFromUtc,
            identity.EffectiveToUtc,
            identity.TenantId,
            identity.PropertyId);

        return OptimizationExecutionEvidence.Create(
            request.TenantId,
            request.PropertyId,
            request.ConfigurationVersion,
            request.EffectiveDateUtc,
            request.OccupancyRate,
            request.ConfidenceThreshold,
            "subsidy-optimizer-v1",
            result.ParticipatingConsumptionHistory.Select(x => new OptimizationMeterInput(
                x.MeterId,
                x.PeriodStart,
                x.PeriodEnd,
                x.TotalConsumptionUnits,
                x.CapturedAtUtc,
                x.MeterType,
                x.MeterStatus,
                x.SanctionedLoad!.Value)).ToArray(),
            request.RatedConsumptions.Select(x => new OptimizationRatingInput(
                x.RatingId, x.MeterId, x.PeriodStart, x.PeriodEnd, x.RatedUnits, x.RatedAmount, x.RatedAtUtc)).ToArray(),
            request.ImportedDatasets.Select(x => new OptimizationImportedDatasetInput(
                x.DatasetId, x.DatasetType, x.SourceSystem, x.Version, x.ImportedAtUtc)).ToArray(),
            new OptimizationPolicySnapshot(
                result.GovernedConfiguration.Policy.PolicyCode,
                MapIdentity(result.GovernedConfiguration.PolicyIdentity),
                result.GovernedConfiguration.Policy.Slabs.Select(x => new OptimizationSubsidySlabSnapshot(
                    x.MaximumUnits, x.SubsidyAmount, x.IsCliff)).ToArray(),
                result.GovernedConfiguration.Policy.SanctionedLoadLimit,
                result.GovernedConfiguration.Policy.SanctionedLoadPenaltyPerUnit,
                result.GovernedConfiguration.Policy.EligibleMeterTypes.ToArray()),
            new OptimizationModelSnapshot(
                result.GovernedConfiguration.Model.ModelCode,
                MapIdentity(result.GovernedConfiguration.ModelIdentity),
                result.GovernedConfiguration.Model.SubsidyWeight,
                result.GovernedConfiguration.Model.CostWeight,
                result.GovernedConfiguration.Model.LoadImpactWeight,
                result.GovernedConfiguration.Model.RiskWeight,
                result.GovernedConfiguration.Model.BoundaryTolerance,
                result.GovernedConfiguration.Model.MaximumScenarioCount),
            new OptimizationStrategySnapshot(
                result.GovernedConfiguration.Strategy.StrategyCode,
                MapIdentity(result.GovernedConfiguration.StrategyIdentity),
                result.GovernedConfiguration.Strategy.ConsumptionFactors.ToArray(),
                result.GovernedConfiguration.Strategy.IncludeSubsidyBoundaries,
                result.GovernedConfiguration.Strategy.PermitCrossMeterMovement,
                result.GovernedConfiguration.Strategy.MaximumCrossMeterMovementFraction),
            result.RankedScenarios.Select(x => new OptimizationScenarioEvidence(
                x.ScenarioCode,
                x.ForecastConsumptionUnits,
                x.ExpectedSubsidy,
                x.ExpectedCost,
                x.SanctionedLoadImpact,
                x.RankScore,
                x.IsFeasible,
                x.InfeasibilityReason,
                x.TriggeredBoundary,
                x.TradeOffSummary,
                x.MeterAllocations.Select(allocation => new OptimizationMeterAllocationEvidence(
                    allocation.MeterId,
                    allocation.BaselineUnits,
                    allocation.AllocatedUnits,
                    allocation.SanctionedLoad,
                    allocation.MovementUnits)).ToArray())).ToArray(),
            selected.ScenarioCode,
            new OptimizationOutcomeEvidence(
                optimizationResult.EstimatedSavings,
                optimizationResult.EstimatedCost,
                optimizationResult.Summary,
                recommendation.RecommendationId.Value,
                recommendation.Title,
                recommendation.Details,
                recommendation.Priority.Value));
    }
}
