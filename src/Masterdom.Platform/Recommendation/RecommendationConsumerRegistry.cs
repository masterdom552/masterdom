namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationConsumerRegistry
{
    private readonly IReadOnlyList<IRecommendationConsumer> _consumers;

    public RecommendationConsumerRegistry(IEnumerable<IRecommendationConsumer>? consumers = null)
    {
        _consumers = consumers?.ToArray() ?? Array.Empty<IRecommendationConsumer>();
    }

    public IReadOnlyList<IRecommendationConsumer> GetOrderedConsumers()
    {
        return _consumers
            .OrderBy(x => x.Order)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public RecommendationConsumerExecutionSummary Execute(
        RecommendationConsumerExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var consumers = GetOrderedConsumers();
        RecommendationConsumerValidation.ValidateRegistrations(consumers);

        if (consumers.Count == 0)
        {
            return RecommendationConsumerExecutionSummary.Empty;
        }

        var results = new List<RecommendationConsumerResult>();
        var failures = new List<string>();

        foreach (var consumer in consumers)
        {
            RecommendationConsumerResult result;

            try
            {
                result = consumer.Consume(context) ??
                    new RecommendationConsumerResult(
                        consumer.Name,
                        succeeded: true,
                        isOptional: consumer.IsOptional,
                        processedRecommendationCount: 0,
                        message: "No consumer result returned.");
            }
            catch (Exception ex) when (consumer.IsOptional)
            {
                failures.Add($"Optional consumer '{consumer.Name}' failed: {ex.Message}");
                results.Add(new RecommendationConsumerResult(
                    consumer.Name,
                    succeeded: false,
                    isOptional: true,
                    processedRecommendationCount: 0,
                    message: ex.Message));

                continue;
            }
            catch (Exception ex)
            {
                failures.Add($"Consumer '{consumer.Name}' failed: {ex.Message}");
                results.Add(new RecommendationConsumerResult(
                    consumer.Name,
                    succeeded: false,
                    isOptional: false,
                    processedRecommendationCount: 0,
                    message: ex.Message));

                if (context.StopOnConsumerFailure)
                {
                    break;
                }

                continue;
            }

            RecommendationConsumerValidation.ValidateResult(consumer, result, context);
            results.Add(result);
        }

        return new RecommendationConsumerExecutionSummary(results, failures);
    }
}
