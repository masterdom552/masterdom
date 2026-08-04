namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationConsumerExecutionSummary
{
    public static RecommendationConsumerExecutionSummary Empty { get; } = new([], []);

    public RecommendationConsumerExecutionSummary(
        IReadOnlyList<RecommendationConsumerResult> results,
        IReadOnlyList<string> failures)
    {
        Results = (results ?? throw new ArgumentNullException(nameof(results))).ToArray();
        Failures = (failures ?? throw new ArgumentNullException(nameof(failures))).ToArray();
    }

    public IReadOnlyList<RecommendationConsumerResult> Results { get; }

    public IReadOnlyList<string> Failures { get; }

    public bool HasFailures => Failures.Count > 0;

    public int ExecutedConsumerCount => Results.Count;

    public int SuccessfulConsumerCount => Results.Count(result => result.Succeeded);

    public int FailedConsumerCount => Results.Count(result => !result.Succeeded);

    public static RecommendationConsumerExecutionSummary Merge(
        RecommendationConsumerExecutionSummary left,
        RecommendationConsumerExecutionSummary right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return new RecommendationConsumerExecutionSummary(
            left.Results.Concat(right.Results).ToArray(),
            left.Failures.Concat(right.Failures).ToArray());
    }
}
