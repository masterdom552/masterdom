namespace Masterdom.Platform.Recommendation;

public static class RecommendationValidation
{
    public static void ValidateProviders(IReadOnlyList<IRecommendationProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        var duplicateNames = providers
            .GroupBy(provider => provider.Name, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateNames.Length > 0)
        {
            throw new RecommendationValidationException(
                $"Duplicate recommendation provider names were found: {string.Join(", ", duplicateNames)}.");
        }
    }

    public static void ValidateRecommendations(
        IRecommendationProvider provider,
        IReadOnlyList<Recommendation> recommendations)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(recommendations);

        if (recommendations.Any(recommendation => recommendation is null))
        {
            throw new RecommendationValidationException(
                $"Provider '{provider.Name}' returned null recommendation entries.");
        }

        var duplicateIds = recommendations
            .GroupBy(recommendation => recommendation.Id)
            .Where(group => group.Count() > 1)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new RecommendationValidationException(
                $"Provider '{provider.Name}' returned duplicate recommendation identifiers.");
        }
    }

    public static void ValidateConsumers(IReadOnlyList<IRecommendationConsumer> consumers)
    {
        RecommendationConsumerValidation.ValidateRegistrations(consumers);
    }

    public static void ValidateConsumerResult(
        IRecommendationConsumer consumer,
        RecommendationConsumerResult result,
        RecommendationConsumerExecutionContext context)
    {
        RecommendationConsumerValidation.ValidateResult(consumer, result, context);
    }
}
