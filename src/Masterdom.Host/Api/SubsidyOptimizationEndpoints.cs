using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using Masterdom.Modules.SubsidyOptimization.Application.Support;
using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Host.Api;

internal static class SubsidyOptimizationEndpoints
{
    public static IEndpointRouteBuilder MapSubsidyOptimizationEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/subsidy-optimization").WithTags("SubsidyOptimization").RequireAuthorization();

        group.MapPost("/runs", ExecuteOptimization);
        group.MapGet("/runs/{optimizationRunId:guid}", GetOptimizationRunById);
        group.MapGet("/runs/{optimizationRunId:guid}/recommendation", GetRecommendation);
        group.MapPost("/runs/{optimizationRunId:guid}/archive", ArchiveOptimizationRun);

        return app;
    }

    internal static IResult ExecuteOptimization(
        ExecuteOptimizationRequest request,
        ICommandHandler<ExecuteSubsidyOptimizationCommand, ExecutionResult<OptimizationRunAggregate>> handler)
    {
        var command = new ExecuteSubsidyOptimizationCommand(
            SubsidyScenario.Create(ScenarioId.Create(request.ScenarioCode), request.ScenarioName, request.ScenarioDescription),
            MeterGroup.Create(
                MeterGroupReference.Create(request.MeterGroupCode, request.ConsumptionHistory.Select(x => x.MeterId).Distinct().ToArray()),
                request.MeterGroupName),
            OptimizationPeriod.Create(request.PeriodStart, request.PeriodEnd),
            new SubsidyMaximizerRequest(
                request.ConsumptionHistory,
                request.RatedConsumptions,
                request.ImportedDatasets,
                request.EffectiveDateUtc,
                request.ConfigurationContextVersion,
                request.OccupancyRate,
                request.ConfidenceThreshold,
                request.TenantId,
                request.PropertyId,
                request.UserId,
                request.PortfolioId,
                request.Language,
                request.SecurityContext,
                null,
                null));

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Created($"/api/subsidy-optimization/runs/{result.Value.Id.Value}", OptimizationRunResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetOptimizationRunById(
        Guid optimizationRunId,
        IQueryHandler<GetOptimizationRunByIdQuery, ExecutionResult<OptimizationRunAggregate>> handler)
    {
        var result = handler.Handle(new GetOptimizationRunByIdQuery(OptimizationRunId.From(optimizationRunId)));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(OptimizationRunResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult GetRecommendation(
        Guid optimizationRunId,
        IQueryHandler<GetOptimizationRunByIdQuery, ExecutionResult<OptimizationRunAggregate>> handler)
    {
        var result = handler.Handle(new GetOptimizationRunByIdQuery(OptimizationRunId.From(optimizationRunId)));
        var recommendation = result.Value?.Recommendations.OrderBy(x => x.Priority.Rank).FirstOrDefault();

        return result.IsSuccess && recommendation is not null
            ? TypedResults.Ok(OptimizationRecommendationResponse.From(result.Value!, recommendation))
            : ApiExecutionResults.ToErrorResult("not_found", "Optimization recommendation was not found.");
    }

    internal static IResult ArchiveOptimizationRun(
        Guid optimizationRunId,
        ArchiveOptimizationRunRequest request,
        ICommandHandler<ArchiveOptimizationRunCommand, ExecutionResult<OptimizationRunAggregate>> handler)
    {
        var result = handler.Handle(new ArchiveOptimizationRunCommand(
            OptimizationRunId.From(optimizationRunId),
            request.ArchivedAtUtc));

        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(OptimizationRunResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record ExecuteOptimizationRequest(
        string ScenarioCode,
        string ScenarioName,
        string ScenarioDescription,
        string MeterGroupCode,
        string MeterGroupName,
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        IReadOnlyCollection<MeteringConsumptionHistoryContract> ConsumptionHistory,
        IReadOnlyCollection<RatedConsumptionContract> RatedConsumptions,
        IReadOnlyCollection<ImportedDatasetReference> ImportedDatasets,
        DateTime EffectiveDateUtc,
        string ConfigurationContextVersion,
        decimal OccupancyRate,
        decimal ConfidenceThreshold,
        string? TenantId,
        string? PropertyId,
        string? UserId,
        string? PortfolioId,
        string? Language,
        string? SecurityContext);

    internal sealed record ArchiveOptimizationRunRequest(DateTime ArchivedAtUtc);

    internal sealed record OptimizationRunResponse(
        Guid Id,
        string ScenarioId,
        string OptimizationStatus,
        int OptimizationVersion,
        DateOnly OptimizationPeriodStartDate,
        DateOnly OptimizationPeriodEndDate,
        DateTime StartedAtUtc,
        DateTime? CompletedAtUtc)
    {
        public static OptimizationRunResponse From(OptimizationRunAggregate run)
        {
            return new OptimizationRunResponse(
                run.Id.Value,
                run.Scenario.ScenarioId.Value,
                run.OptimizationStatus.Value,
                run.OptimizationVersion.Value,
                run.OptimizationPeriod.StartDate,
                run.OptimizationPeriod.EndDate,
                run.StartedAtUtc,
                run.CompletedAtUtc);
        }
    }

    internal sealed record OptimizationRecommendationResponse(
        Guid RunId,
        Guid RecommendationId,
        string Title,
        string Details,
        string Priority,
        string SelectedScenario,
        string PolicyVersion,
        string ModelVersion,
        string StrategyVersion)
    {
        public static OptimizationRecommendationResponse From(
            OptimizationRunAggregate run,
            OptimizationRecommendation recommendation)
        {
            var evidence = run.ExecutionEvidence
                ?? throw new InvalidOperationException("Optimization execution evidence is unavailable.");

            return new OptimizationRecommendationResponse(
                run.Id.Value,
                recommendation.RecommendationId.Value,
                recommendation.Title,
                recommendation.Details,
                recommendation.Priority.Value,
                evidence.SelectedScenarioCode,
                $"{evidence.Policy.Code}:v{evidence.Policy.Version}",
                $"{evidence.Model.Code}:v{evidence.Model.Version}",
                $"{evidence.Strategy.Code}:v{evidence.Strategy.Version}");
        }
    }
}
