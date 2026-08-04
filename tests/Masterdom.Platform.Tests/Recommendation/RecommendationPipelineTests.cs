using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Recommendation;
using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;
using RecommendationModel = Masterdom.Platform.Recommendation.Recommendation;

namespace Masterdom.Platform.Tests.RecommendationPlatform;

public sealed class RecommendationPipelineTests
{
    [Fact]
    public void Pipeline_ShouldCreateBundle_FromProviderComposition()
    {
        var context = CreateContext();
        var pipeline = CreatePipeline(
            providers:
            [
                new StubRecommendationProvider("provider-b", order: 20, priority: 1),
                new StubRecommendationProvider("provider-a", order: 10, priority: 1)
            ],
            consumers: [new TrackingConsumer("consumer-a", 10, 1)]);

        var session = pipeline.CreateSession(context);
        var bundle = pipeline.BuildBundle(context, session);

        Assert.Equal(RecommendationBundleStatus.Finalized, bundle.Status);
        Assert.Equal(2, bundle.Recommendations.Count);
        Assert.Equal(context.Metadata.EffectiveDateUtc, bundle.EffectiveDateUtc);
    }

    [Fact]
    public void Pipeline_WhenOptionalProviderFails_ShouldContinue()
    {
        var context = CreateContext();
        var pipeline = CreatePipeline(
            providers:
            [
                new StubRecommendationProvider("provider-a", order: 10, priority: 1),
                new FailingRecommendationProvider("provider-b", order: 20, priority: 1, isOptional: true)
            ],
            consumers: [new TrackingConsumer("consumer-a", 10, 1)]);

        var session = pipeline.CreateSession(context);
        var bundle = pipeline.BuildBundle(context, session);

        Assert.Single(bundle.Recommendations);
    }

    [Fact]
    public void Pipeline_ShouldCreateDecision_AndPersistRepositories()
    {
        var context = CreateContext();
        var recommendationRepository = new InMemoryRecommendationRepository();
        var decisionRepository = new InMemoryDecisionRepository();
        var sessionRepository = new InMemoryOptimizationSessionRepository();

        var pipeline = new RecommendationPipeline(
            new RecommendationProviderRegistry([new StubRecommendationProvider("provider-a", order: 10, priority: 1)]),
            new RecommendationConsumerRegistry([new TrackingConsumer("consumer-a", 10, 1)]),
            [new PassThroughDecisionHandler()],
            recommendationRepository,
            decisionRepository,
            sessionRepository);

        var session = pipeline.CreateSession(context);
        var bundle = pipeline.BuildBundle(context, session);
        var decision = pipeline.CreateDecision(
            context,
            bundle,
            DecisionType.Create("approval"),
            DecisionReason.Create("generic review"),
            approve: true);

        var storedSession = sessionRepository.Get(session.Id);
        var storedBundle = recommendationRepository.GetBundle(bundle.Id);
        var storedDecision = decisionRepository.Get(decision.Id);

        Assert.NotNull(storedSession);
        Assert.NotNull(storedBundle);
        Assert.NotNull(storedDecision);
        Assert.Equal(DecisionStatus.Approved, storedDecision!.Status);
        Assert.Equal(RecommendationBundleStatus.Decided, storedBundle!.Status);
        Assert.Equal(decision.Id.Value, storedBundle.DecisionId!.Value);
    }

    [Fact]
    public void Recommendation_ShouldTransitionStatuses_Immutably()
    {
        var recommendation = CreateRecommendation("provider-a")
            .MarkProposed(DateTime.UtcNow)
            .Accept("approved", DateTime.UtcNow);

        Assert.Equal(RecommendationStatus.Accepted, recommendation.Status);
        Assert.Equal("approved", recommendation.StatusReason);
    }

    [Fact]
    public void Session_ShouldTransitionStatuses_Immutably()
    {
        var metadata = new OptimizationSessionMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: DateTime.UtcNow,
            contextVersion: "1",
            recommendationVersion: "v1");

        var session = OptimizationSession.Create(OptimizationSessionId.New(), metadata)
            .Start(DateTime.UtcNow)
            .Complete(DateTime.UtcNow);

        Assert.Equal(OptimizationSessionStatus.Completed, session.Status);
    }

    [Fact]
    public void Decision_ShouldTransitionStatuses_Immutably()
    {
        var decision = Decision.CreatePending(
                DecisionId.New(),
                DecisionType.Create("approval"),
                DecisionReason.Create("generic review"),
                RecommendationBundleId.New(),
                DateTime.UtcNow)
            .Approve(DateTime.UtcNow)
            .Apply(DateTime.UtcNow);

        Assert.Equal(DecisionStatus.Applied, decision.Status);
    }

    [Fact]
    public void Consumers_ShouldExecuteInOrderThenPriority()
    {
        var context = CreateContext();
        var capture = new List<string>();
        var sessionMetadata = new OptimizationSessionMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: context.Metadata.EffectiveDateUtc,
            contextVersion: context.Version.ToString(),
            recommendationVersion: "v1");
        var session = OptimizationSession.Create(OptimizationSessionId.New(), sessionMetadata).Start(DateTime.UtcNow);

        var bundle = RecommendationBundle
            .CreateDraft(RecommendationBundleId.New(), [CreateRecommendation("provider-a")], DateTime.UtcNow, context.Metadata.EffectiveDateUtc, "v1")
            .Open()
            .FinalizeBundle();

        var pipeline = new RecommendationPipeline(
            new RecommendationProviderRegistry(),
            new RecommendationConsumerRegistry(
            [
                new TrackingConsumer("c-3", order: 20, priority: 1, captureOrder: capture),
                new TrackingConsumer("c-2", order: 10, priority: 1, captureOrder: capture),
                new TrackingConsumer("c-1", order: 10, priority: 5, captureOrder: capture)
            ]),
            [],
            new InMemoryRecommendationRepository(),
            new InMemoryDecisionRepository(),
            new InMemoryOptimizationSessionRepository());

        var summary = pipeline.ExecuteConsumers(context, session, bundle);

        Assert.Equal(["c-1", "c-2", "c-3"], capture.Distinct().ToArray());
        Assert.Equal(3, summary.ExecutedConsumerCount);
    }

    [Fact]
    public void Consumers_WhenOptionalConsumerFails_ShouldIsolateFailure()
    {
        var context = CreateContext();
        var sessionMetadata = new OptimizationSessionMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: context.Metadata.EffectiveDateUtc,
            contextVersion: context.Version.ToString(),
            recommendationVersion: "v1");
        var session = OptimizationSession.Create(OptimizationSessionId.New(), sessionMetadata).Start(DateTime.UtcNow);

        var bundle = RecommendationBundle
            .CreateDraft(RecommendationBundleId.New(), [CreateRecommendation("provider-a")], DateTime.UtcNow, context.Metadata.EffectiveDateUtc, "v1")
            .Open()
            .FinalizeBundle();

        var capture = new List<string>();
        var pipeline = new RecommendationPipeline(
            new RecommendationProviderRegistry(),
            new RecommendationConsumerRegistry(
            [
                new FailingConsumer("optional-failure", 10, 1, isOptional: true),
                new TrackingConsumer("next-consumer", 20, 1, captureOrder: capture)
            ]),
            [],
            new InMemoryRecommendationRepository(),
            new InMemoryDecisionRepository(),
            new InMemoryOptimizationSessionRepository());

        var summary = pipeline.ExecuteConsumers(context, session, bundle);

        Assert.Single(summary.Failures);
        Assert.Contains("optional-failure", summary.Failures[0], StringComparison.Ordinal);
        Assert.Contains("next-consumer", capture);
    }

    [Fact]
    public void Consumers_ShouldNotMutateRecommendationContextOrDecision()
    {
        var context = CreateContext();
        var recommendation = CreateRecommendation("provider-a").MarkProposed(DateTime.UtcNow);
        var originalStatus = recommendation.Status;
        var originalContextEffectiveDate = context.Metadata.EffectiveDateUtc;

        var sessionMetadata = new OptimizationSessionMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: context.Metadata.EffectiveDateUtc,
            contextVersion: context.Version.ToString(),
            recommendationVersion: "v1");
        var session = OptimizationSession.Create(OptimizationSessionId.New(), sessionMetadata).Start(DateTime.UtcNow);

        var bundle = RecommendationBundle
            .CreateDraft(RecommendationBundleId.New(), [recommendation], DateTime.UtcNow, context.Metadata.EffectiveDateUtc, "v1")
            .Open()
            .FinalizeBundle();

        var decision = Decision.CreatePending(
            DecisionId.New(),
            DecisionType.Create("approval"),
            DecisionReason.Create("reason"),
            bundle.Id,
            DateTime.UtcNow);

        var pipeline = new RecommendationPipeline(
            new RecommendationProviderRegistry(),
            new RecommendationConsumerRegistry([new InspectingConsumer("inspector", 10, 1)]),
            [],
            new InMemoryRecommendationRepository(),
            new InMemoryDecisionRepository(),
            new InMemoryOptimizationSessionRepository());

        var summary = pipeline.ExecuteConsumers(context, session, bundle, decision);

        Assert.Single(summary.Results);
        Assert.Equal(originalStatus, recommendation.Status);
        Assert.Equal(originalContextEffectiveDate, context.Metadata.EffectiveDateUtc);
        Assert.Equal(DecisionStatus.Pending, decision.Status);
    }

    [Fact]
    public void Consumers_WithDuplicateRegistrations_ShouldThrowValidationException()
    {
        var context = CreateContext();
        var sessionMetadata = new OptimizationSessionMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: context.Metadata.EffectiveDateUtc,
            contextVersion: context.Version.ToString(),
            recommendationVersion: "v1");
        var session = OptimizationSession.Create(OptimizationSessionId.New(), sessionMetadata).Start(DateTime.UtcNow);

        var bundle = RecommendationBundle
            .CreateDraft(RecommendationBundleId.New(), [CreateRecommendation("provider-a")], DateTime.UtcNow, context.Metadata.EffectiveDateUtc, "v1")
            .Open()
            .FinalizeBundle();

        var pipeline = new RecommendationPipeline(
            new RecommendationProviderRegistry(),
            new RecommendationConsumerRegistry(
            [
                new TrackingConsumer("dup", 10, 1),
                new TrackingConsumer("dup", 20, 1)
            ]),
            [],
            new InMemoryRecommendationRepository(),
            new InMemoryDecisionRepository(),
            new InMemoryOptimizationSessionRepository());

        Assert.Throws<RecommendationValidationException>(() =>
            pipeline.ExecuteConsumers(context, session, bundle));
    }

    [Fact]
    public void Consumers_WithInvalidPriority_ShouldThrowValidationException()
    {
        var consumers =
            new IRecommendationConsumer[]
            {
                new TrackingConsumer("bad-priority", 10, 0)
            };

        Assert.Throws<RecommendationValidationException>(() =>
            RecommendationConsumerValidation.ValidateRegistrations(consumers));
    }

    private static RecommendationPipeline CreatePipeline(
        IReadOnlyList<IRecommendationProvider> providers,
        IReadOnlyList<IRecommendationConsumer>? consumers = null)
    {
        return new RecommendationPipeline(
            new RecommendationProviderRegistry(providers),
            new RecommendationConsumerRegistry(consumers),
            [new PassThroughDecisionHandler()],
            new InMemoryRecommendationRepository(),
            new InMemoryDecisionRepository(),
            new InMemoryOptimizationSessionRepository());
    }

    private static BusinessContextModel CreateContext()
    {
        var effectiveDate = new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc);

        var metadata = new BusinessContextMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: effectiveDate,
            configurationVersion: "cfg-v1",
            language: "en-US",
            securityContext: "superuser",
            userId: "u-1",
            portfolioId: "p-1",
            providerExecutionOrder: Array.Empty<string>(),
            warnings: Array.Empty<string>());

        return new BusinessContextModel(
            version: BusinessContextVersion.BaselineV1,
            metadata: metadata,
            snapshots: new Dictionary<string, BusinessContextSnapshot>(),
            references: Array.Empty<BusinessContextReference>());
    }

    private static RecommendationModel CreateRecommendation(string source)
    {
        return RecommendationModel.CreateDraft(
            id: RecommendationId.New(),
            type: RecommendationType.Create("generic"),
            priority: RecommendationPriority.Create(10),
            confidence: RecommendationConfidence.Create(0.92m),
            evidence: new RecommendationEvidence("EVID-1", "Source evidence"),
            explanation: new RecommendationExplanation("Generic explanation", ["step-1"]),
            metadata: new RecommendationMetadata(
                createdAtUtc: DateTime.UtcNow,
                effectiveDateUtc: DateTime.UtcNow,
                version: "v1",
                source: source));
    }

    private sealed class StubRecommendationProvider : IRecommendationProvider
    {
        public StubRecommendationProvider(string name, int order, int priority)
        {
            Name = name;
            Order = order;
            Priority = priority;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => true;

        public IReadOnlyList<RecommendationModel> Provide(BusinessContextModel context, OptimizationSession session)
        {
            _ = session;

            var recommendation = RecommendationModel.CreateDraft(
                    id: RecommendationId.New(),
                    type: RecommendationType.Create("generic"),
                    priority: RecommendationPriority.Create(10),
                    confidence: RecommendationConfidence.Create(0.85m),
                    evidence: new RecommendationEvidence("EVID-1", "Generic evidence"),
                    explanation: new RecommendationExplanation("Generic explanation"),
                    metadata: new RecommendationMetadata(
                        createdAtUtc: DateTime.UtcNow,
                        effectiveDateUtc: context.Metadata.EffectiveDateUtc,
                        version: context.Version.ToString(),
                        source: Name))
                .MarkProposed(DateTime.UtcNow);

            return [recommendation];
        }
    }

    private sealed class FailingRecommendationProvider : IRecommendationProvider
    {
        private readonly bool _isOptional;

        public FailingRecommendationProvider(string name, int order, int priority, bool isOptional)
        {
            Name = name;
            Order = order;
            Priority = priority;
            _isOptional = isOptional;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => _isOptional;

        public IReadOnlyList<RecommendationModel> Provide(BusinessContextModel context, OptimizationSession session)
        {
            _ = context;
            _ = session;
            throw new InvalidOperationException("simulated provider failure");
        }
    }

    private sealed class PassThroughDecisionHandler : IDecisionHandler
    {
        public string Name => "passthrough";

        public int Order => 10;

        public int Priority => 1;

        public Decision Handle(Decision decision, RecommendationBundle bundle, BusinessContextModel context)
        {
            _ = bundle;
            _ = context;
            return decision;
        }
    }

    private sealed class TrackingConsumer : IRecommendationConsumer
    {
        private readonly IList<string>? _captureOrder;

        public TrackingConsumer(string name, int order, int priority, IList<string>? captureOrder = null)
        {
            Name = name;
            Order = order;
            Priority = priority;
            _captureOrder = captureOrder;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => true;

        public RecommendationConsumerResult Consume(RecommendationConsumerExecutionContext context)
        {
            _captureOrder?.Add(Name);

            return new RecommendationConsumerResult(
                consumerName: Name,
                succeeded: true,
                isOptional: IsOptional,
            processedRecommendationCount: 1,
                message: "ok");
        }
    }

    private sealed class FailingConsumer : IRecommendationConsumer
    {
        private readonly bool _isOptional;

        public FailingConsumer(string name, int order, int priority, bool isOptional)
        {
            Name = name;
            Order = order;
            Priority = priority;
            _isOptional = isOptional;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => _isOptional;

        public RecommendationConsumerResult Consume(RecommendationConsumerExecutionContext context)
        {
            _ = context;
            throw new InvalidOperationException("simulated consumer failure");
        }
    }

    private sealed class InspectingConsumer : IRecommendationConsumer
    {
        public InspectingConsumer(string name, int order, int priority)
        {
            Name = name;
            Order = order;
            Priority = priority;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => false;

        public RecommendationConsumerResult Consume(RecommendationConsumerExecutionContext context)
        {
            Assert.NotNull(context.BusinessContext);
            Assert.NotNull(context.RecommendationBundle);
            Assert.NotNull(context.Decision);
            Assert.NotNull(context.Recommendation);
            Assert.NotEqual(Guid.Empty, context.CorrelationId);

            return new RecommendationConsumerResult(
                consumerName: Name,
                succeeded: true,
                isOptional: IsOptional,
                processedRecommendationCount: 1,
                message: "inspected");
        }
    }
}
