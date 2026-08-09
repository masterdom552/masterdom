using Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

namespace Masterdom.Modules.SubsidyOptimization.Application.Commands;

public sealed record ArchiveOptimizationRunCommand(
    OptimizationRunId OptimizationRunId,
    DateTime ArchivedAtUtc);
