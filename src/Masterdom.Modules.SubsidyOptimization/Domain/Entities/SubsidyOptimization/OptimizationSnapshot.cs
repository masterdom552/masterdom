using Masterdom.Core.Primitives;

namespace Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization;

public sealed class OptimizationSnapshot : ValueObject
{
    private OptimizationSnapshot(
        Guid snapshotId,
        OptimizationVersion version,
        DateTime capturedAtUtc,
        OptimizationResult optimizationResult,
        ConsumptionForecast consumptionForecast,
        RecommendationSet recommendationSet)
    {
        SnapshotId = snapshotId;
        Version = version;
        CapturedAtUtc = capturedAtUtc;
        OptimizationResult = optimizationResult;
        ConsumptionForecast = consumptionForecast;
        RecommendationSet = recommendationSet;
    }

    public Guid SnapshotId { get; }

    public OptimizationVersion Version { get; }

    public DateTime CapturedAtUtc { get; }

    public OptimizationResult OptimizationResult { get; }

    public ConsumptionForecast ConsumptionForecast { get; }

    public RecommendationSet RecommendationSet { get; }

    public static OptimizationSnapshot Create(
        OptimizationVersion version,
        DateTime capturedAtUtc,
        OptimizationResult optimizationResult,
        ConsumptionForecast consumptionForecast,
        RecommendationSet recommendationSet)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(optimizationResult);
        ArgumentNullException.ThrowIfNull(consumptionForecast);
        ArgumentNullException.ThrowIfNull(recommendationSet);

        if (capturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Snapshot timestamp must be UTC.");
        }

        return new OptimizationSnapshot(
            Guid.CreateVersion7(),
            version,
            capturedAtUtc,
            optimizationResult,
            consumptionForecast,
            recommendationSet);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SnapshotId;
        yield return Version;
        yield return CapturedAtUtc;
        yield return OptimizationResult;
        yield return ConsumptionForecast;
        yield return RecommendationSet;
    }
}
