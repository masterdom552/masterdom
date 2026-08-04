using Masterdom.Infrastructure;
using Masterdom.Modules.SubsidyOptimization.Application.Maximizer;
using Masterdom.Modules.SubsidyOptimization.Contracts.Metering;
using Masterdom.Modules.SubsidyOptimization.Contracts.UtilityRating;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.Recommendation;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Infrastructure.Tests.SubsidyOptimization;

public sealed class SubsidyMaximizerRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveSubsidyMaximizerServices()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ISubsidyMaximizerService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICalculationRuntime>());
        Assert.NotNull(scope.ServiceProvider.GetService<SubsidyCalculationRuntimeInvoker>());
        Assert.NotNull(scope.ServiceProvider.GetService<ConsumptionEstimator>());
        Assert.NotNull(scope.ServiceProvider.GetService<ForecastEngine>());
        Assert.NotNull(scope.ServiceProvider.GetService<ScenarioGenerator>());
        Assert.NotNull(scope.ServiceProvider.GetService<ScenarioEvaluator>());
        Assert.NotNull(scope.ServiceProvider.GetService<ConfidenceScorer>());
        Assert.NotNull(scope.ServiceProvider.GetService<RecommendationGenerator>());
        Assert.NotNull(scope.ServiceProvider.GetService<OptimizationSessionBuilder>());
    }

    [Fact]
    public void RuntimeComposition_ShouldExecuteSubsidyMaximizer()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<ISubsidyMaximizerService>();

        var result = service.Execute(new SubsidyMaximizerRequest(
            ConsumptionHistory:
            [
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 150m, DateTime.UtcNow),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28), 148m, DateTime.UtcNow),
                new MeteringConsumptionHistoryContract(Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 145m, DateTime.UtcNow)
            ],
            RatedConsumptions:
            [
                new RatedConsumptionContract(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), 144m, 44m, DateTime.UtcNow)
            ],
            ImportedDatasets:
            [
                new ImportedDatasetReference("dataset-1", "consumption", "import-export", "v1", DateTime.UtcNow)
            ],
            EffectiveDateUtc: new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
            ConfigurationVersion: "cfg-v1",
            OccupancyRate: 0.93m,
            ConfidenceThreshold: 0.50m,
            TenantId: "tenant-1",
            PropertyId: "property-1",
            UserId: "user-1",
            PortfolioId: "portfolio-1",
            Language: "en-US",
            SecurityContext: "superuser",
            OptimizationModel: "deterministic-v1",
            OptimizationStrategy: "weighted-threshold"));

        Assert.NotNull(result.BusinessContext);
        Assert.Equal("cfg-v1", result.BusinessContext.Metadata.ConfigurationVersion);
        Assert.Equal(RecommendationBundleStatus.Finalized, result.RecommendationBundle.Status);
        Assert.NotEmpty(result.RecommendationBundle.Recommendations);
        Assert.Equal(OptimizationSessionStatus.Completed, result.OptimizationSession.Status);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddPropertyBusinessCapabilityRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }
}
