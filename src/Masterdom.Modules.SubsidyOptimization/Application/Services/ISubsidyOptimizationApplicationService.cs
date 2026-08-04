using Masterdom.Modules.SubsidyOptimization.Application.Commands;
using Masterdom.Modules.SubsidyOptimization.Application.Queries;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Modules.SubsidyOptimization.Application.Services;

public interface ISubsidyOptimizationApplicationService
{
    OptimizationRunAggregate StartOptimization(StartOptimizationCommand command);

    OptimizationRunAggregate CompleteOptimization(CompleteOptimizationCommand command);

    OptimizationRunAggregate CreateScenarioVersion(CreateScenarioVersionCommand command);

    OptimizationRunAggregate ArchiveRecommendation(ArchiveRecommendationCommand command);

    OptimizationRunAggregate? GetOptimizationRun(GetOptimizationRunByIdQuery query);

    OptimizationRunAggregate? GetLatestOptimizationRun(GetLatestOptimizationRunQuery query);
}
