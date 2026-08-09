using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Commands;

public sealed record ExecuteSubsidyOptimizationCommand(
    SubsidyScenario Scenario,
    MeterGroup MeterGroup,
    OptimizationPeriod OptimizationPeriod,
    SubsidyMaximizerRequest Request);
