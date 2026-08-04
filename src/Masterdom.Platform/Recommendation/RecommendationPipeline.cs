using Masterdom.Platform.BusinessContext;
using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;

namespace Masterdom.Platform.Recommendation;

public sealed class RecommendationPipeline
{
    private readonly RecommendationProviderRegistry _providerRegistry;
    private readonly RecommendationConsumerRegistry _consumerRegistry;
    private readonly IEnumerable<IDecisionHandler> _decisionHandlers;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IDecisionRepository _decisionRepository;
    private readonly IOptimizationSessionRepository _sessionRepository;

    public RecommendationPipeline(
        RecommendationProviderRegistry providerRegistry,
        RecommendationConsumerRegistry consumerRegistry,
        IEnumerable<IDecisionHandler> decisionHandlers,
        IRecommendationRepository recommendationRepository,
        IDecisionRepository decisionRepository,
        IOptimizationSessionRepository sessionRepository)
    {
        _providerRegistry = providerRegistry ?? throw new ArgumentNullException(nameof(providerRegistry));
        _consumerRegistry = consumerRegistry ?? throw new ArgumentNullException(nameof(consumerRegistry));
        _decisionHandlers = decisionHandlers ?? throw new ArgumentNullException(nameof(decisionHandlers));
        _recommendationRepository = recommendationRepository ?? throw new ArgumentNullException(nameof(recommendationRepository));
        _decisionRepository = decisionRepository ?? throw new ArgumentNullException(nameof(decisionRepository));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public OptimizationSession CreateSession(BusinessContextModel context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var metadata = new OptimizationSessionMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: context.Metadata.EffectiveDateUtc,
            contextVersion: context.Version.ToString(),
            recommendationVersion: "v1",
            attributes: context.Metadata.Attributes);

        var session = OptimizationSession
            .Create(OptimizationSessionId.New(), metadata)
            .Start(DateTime.UtcNow);

        _sessionRepository.Save(session);

        return session;
    }

    public RecommendationBundle BuildBundle(BusinessContextModel context, OptimizationSession session)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(session);

        var providers = _providerRegistry.GetOrderedProviders();
        RecommendationValidation.ValidateProviders(providers);

        var recommendations = new List<Recommendation>();

        foreach (var provider in providers)
        {
            IReadOnlyList<Recommendation> produced;

            try
            {
                produced = provider.Provide(context, session) ?? Array.Empty<Recommendation>();
            }
            catch (Exception) when (provider.IsOptional)
            {
                continue;
            }

            RecommendationValidation.ValidateRecommendations(provider, produced);
            recommendations.AddRange(produced);
        }

        var ordered = recommendations
            .OrderBy(x => x.Priority.Value)
            .ThenBy(x => x.Id.Value)
            .ToArray();

        var bundle = RecommendationBundle
            .CreateDraft(
                RecommendationBundleId.New(),
                ordered,
                createdAtUtc: DateTime.UtcNow,
                effectiveDateUtc: context.Metadata.EffectiveDateUtc,
                version: context.Version.ToString())
            .Open()
            .FinalizeBundle();

        _recommendationRepository.SaveBundle(bundle);
        _ = ExecuteConsumers(context, session, bundle);

        return bundle;
    }

    public Decision CreateDecision(
        BusinessContextModel context,
        RecommendationBundle bundle,
        DecisionType type,
        DecisionReason reason,
        bool approve)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(bundle);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(reason);

        var decision = Decision.CreatePending(
            DecisionId.New(),
            type,
            reason,
            bundle.Id,
            DateTime.UtcNow);

        var handlers = _decisionHandlers
            .OrderBy(x => x.Order)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var handler in handlers)
        {
            decision = handler.Handle(decision, bundle, context);
        }

        decision = approve
            ? decision.Approve(DateTime.UtcNow)
            : decision.Reject(DateTime.UtcNow);

        _decisionRepository.Save(decision);
        _recommendationRepository.SaveBundle(bundle.MarkDecided(decision.Id));

        return decision;
    }

    public RecommendationConsumerExecutionSummary ExecuteConsumers(
        BusinessContextModel context,
        OptimizationSession session,
        RecommendationBundle bundle,
        Decision? decision = null,
        bool stopOnConsumerFailure = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(bundle);

        var summary = RecommendationConsumerExecutionSummary.Empty;
        var correlationId = Guid.CreateVersion7();

        foreach (var recommendation in bundle.Recommendations)
        {
            var executionContext = new RecommendationConsumerExecutionContext(
                recommendation: recommendation,
                recommendationBundle: bundle,
                optimizationSession: session,
                businessContext: context,
                correlationId: correlationId,
                executionTimestampUtc: DateTime.UtcNow,
                effectiveDateUtc: context.Metadata.EffectiveDateUtc,
                configurationVersion: context.Metadata.ConfigurationVersion,
                decision: decision,
                stopOnConsumerFailure: stopOnConsumerFailure,
                cancellationToken: cancellationToken);

            var recommendationSummary = _consumerRegistry.Execute(executionContext);
            summary = RecommendationConsumerExecutionSummary.Merge(summary, recommendationSummary);

            if (stopOnConsumerFailure && recommendationSummary.HasFailures)
            {
                break;
            }
        }

        return summary;
    }
}
