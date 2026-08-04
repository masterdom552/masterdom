using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Queries;

public sealed record GetLatestOptimizationRunQuery(
    ScenarioId ScenarioId,
    OptimizationPeriod OptimizationPeriod);
