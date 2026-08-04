using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;

namespace Masterdom.Modules.SubsidyOptimization.Application.Maximizer;

public sealed record SubsidyMaximizerRequest(
    IReadOnlyCollection<MeteringConsumptionHistoryContract> ConsumptionHistory,
    IReadOnlyCollection<RatedConsumptionContract> RatedConsumptions,
    IReadOnlyCollection<ImportedDatasetReference> ImportedDatasets,
    DateTime EffectiveDateUtc,
    string ConfigurationVersion,
    decimal OccupancyRate,
    decimal ConfidenceThreshold,
    string? TenantId,
    string? PropertyId,
    string? UserId,
    string? PortfolioId,
    string? Language,
    string? SecurityContext,
    string? OptimizationModel,
    string? OptimizationStrategy);
