namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationConsumerResult
{
    public RecommendationConsumerResult(
        string consumerName,
        bool succeeded,
        bool isOptional,
        int processedRecommendationCount,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(consumerName))
        {
            throw new ArgumentException("Consumer name cannot be empty.", nameof(consumerName));
        }

        if (processedRecommendationCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(processedRecommendationCount));
        }

        ConsumerName = consumerName.Trim();
        Succeeded = succeeded;
        IsOptional = isOptional;
        ProcessedRecommendationCount = processedRecommendationCount;
        Message = message;
    }

    public string ConsumerName { get; }

    public bool Succeeded { get; }

    public bool IsOptional { get; }

    public int ProcessedRecommendationCount { get; }

    public string? Message { get; }
}
