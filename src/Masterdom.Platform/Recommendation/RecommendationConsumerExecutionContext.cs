using System.Collections.ObjectModel;
using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;

namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationConsumerExecutionContext
{
    public RecommendationConsumerExecutionContext(
        Recommendation recommendation,
        RecommendationBundle recommendationBundle,
        OptimizationSession optimizationSession,
        BusinessContextModel businessContext,
        Guid correlationId,
        DateTime executionTimestampUtc,
        DateTime effectiveDateUtc,
        string? configurationVersion,
        Decision? decision = null,
        bool stopOnConsumerFailure = false,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        ArgumentNullException.ThrowIfNull(recommendation);
        ArgumentNullException.ThrowIfNull(recommendationBundle);
        ArgumentNullException.ThrowIfNull(optimizationSession);
        ArgumentNullException.ThrowIfNull(businessContext);

        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("Correlation id cannot be empty.", nameof(correlationId));
        }

        if (executionTimestampUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Execution timestamp must be UTC.");
        }

        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Effective date must be UTC.");
        }

        Recommendation = recommendation;
        RecommendationBundle = recommendationBundle;
        OptimizationSession = optimizationSession;
        BusinessContext = businessContext;
        CorrelationId = correlationId;
        ExecutionTimestampUtc = executionTimestampUtc;
        EffectiveDateUtc = effectiveDateUtc;
        ConfigurationVersion = configurationVersion;
        Decision = decision;
        StopOnConsumerFailure = stopOnConsumerFailure;
        CancellationToken = cancellationToken;
        Attributes = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>(attributes ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase));
    }

    public Recommendation Recommendation { get; }

    public RecommendationBundle RecommendationBundle { get; }

    public OptimizationSession OptimizationSession { get; }

    public BusinessContextModel BusinessContext { get; }

    public Guid CorrelationId { get; }

    public DateTime ExecutionTimestampUtc { get; }

    public DateTime EffectiveDateUtc { get; }

    public string? ConfigurationVersion { get; }

    public Decision? Decision { get; }

    public bool StopOnConsumerFailure { get; }

    public CancellationToken CancellationToken { get; }

    public IReadOnlyDictionary<string, string> Attributes { get; }
}
