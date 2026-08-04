namespace Masterdom.Platform.Recommendation;

public static class RecommendationConsumerValidation
{
    public static void ValidateRegistrations(IReadOnlyList<IRecommendationConsumer> consumers)
    {
        ArgumentNullException.ThrowIfNull(consumers);

        var invalidNames = consumers
            .Where(consumer => string.IsNullOrWhiteSpace(consumer.Name))
            .ToArray();

        if (invalidNames.Length > 0)
        {
            throw new RecommendationValidationException("Consumer registrations include empty names.");
        }

        var invalidPriorities = consumers
            .Where(consumer => consumer.Priority < 1 || consumer.Priority > 100)
            .ToArray();

        if (invalidPriorities.Length > 0)
        {
            throw new RecommendationValidationException("Consumer registrations include invalid priorities.");
        }

        var duplicateNames = consumers
            .GroupBy(consumer => consumer.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            throw new RecommendationValidationException(
                $"Duplicate recommendation consumer names were found: {string.Join(", ", duplicateNames)}.");
        }

        var orderingConflicts = consumers
            .GroupBy(consumer => new { consumer.Order, consumer.Priority })
            .Where(group => group.Count() > 1)
            .ToArray();

        if (orderingConflicts.Length > 0)
        {
            throw new RecommendationValidationException(
                "Recommendation consumer registrations contain ordering conflicts.");
        }

        var priorityConflicts = consumers
            .GroupBy(consumer => new { consumer.Order, consumer.Name, consumer.Priority })
            .Where(group => group.Count() > 1)
            .ToArray();

        if (priorityConflicts.Length > 0)
        {
            throw new RecommendationValidationException(
                "Recommendation consumer registrations contain priority conflicts.");
        }
    }

    public static void ValidateResult(
        IRecommendationConsumer consumer,
        RecommendationConsumerResult result,
        RecommendationConsumerExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(context);

        if (!string.Equals(consumer.Name, result.ConsumerName, StringComparison.OrdinalIgnoreCase))
        {
            throw new RecommendationValidationException(
                $"Consumer '{consumer.Name}' returned mismatched result name '{result.ConsumerName}'.");
        }

        if (result.ProcessedRecommendationCount < 0 || result.ProcessedRecommendationCount > 1)
        {
            throw new RecommendationValidationException(
                $"Consumer '{consumer.Name}' reported an invalid processed recommendation count.");
        }
    }
}
