using Masterdom.Infrastructure;
using Masterdom.Platform.BusinessContext;
using Masterdom.Platform.Recommendation;
using Microsoft.Extensions.DependencyInjection;
using BusinessContextModel = Masterdom.Platform.BusinessContext.BusinessContext;
using RecommendationModel = Masterdom.Platform.Recommendation.Recommendation;

namespace Masterdom.Platform.Infrastructure.Tests.RecommendationPlatform;

public sealed class RecommendationRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveRecommendationServices()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<RecommendationPipeline>());
        Assert.NotNull(scope.ServiceProvider.GetService<IRecommendationRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IDecisionRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IOptimizationSessionRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<RecommendationConsumerRegistry>());
        Assert.NotNull(scope.ServiceProvider.GetService<RecommendationConsumerExecutionSummary>());
    }

    [Fact]
    public void RuntimeComposition_ShouldExecuteRecommendationPipeline()
    {
        using var provider = BuildProvider(services =>
        {
            services.AddScoped<IRecommendationProvider>(_ => new StubRecommendationProvider("provider-a", 10, 1));
            services.AddScoped<IDecisionHandler>(_ => new PassThroughDecisionHandler());
            services.AddScoped<IRecommendationConsumer>(_ => new TrackingConsumer("consumer-a", 10, 1));
            services.AddScoped<IRecommendationConsumer>(_ => new TrackingConsumer("consumer-b", 20, 1));
        });

        using var scope = provider.CreateScope();

        var pipeline = scope.ServiceProvider.GetRequiredService<RecommendationPipeline>();
        var context = CreateContext();

        var session = pipeline.CreateSession(context);
        var bundle = pipeline.BuildBundle(context, session);
        var decision = pipeline.CreateDecision(
            context,
            bundle,
            DecisionType.Create("approval"),
            DecisionReason.Create("generic review"),
            approve: true);

        Assert.Equal(OptimizationSessionStatus.Running, session.Status);
        Assert.Equal(RecommendationBundleStatus.Finalized, bundle.Status);
        Assert.Equal(DecisionStatus.Approved, decision.Status);
    }

    [Fact]
    public void RuntimeComposition_ShouldIsolateOptionalConsumerFailure()
    {
        using var provider = BuildProvider(services =>
        {
            services.AddScoped<IRecommendationProvider>(_ => new StubRecommendationProvider("provider-a", 10, 1));
            services.AddScoped<IDecisionHandler>(_ => new PassThroughDecisionHandler());
            services.AddScoped<IRecommendationConsumer>(_ => new FailingConsumer("optional-consumer", 10, 1, isOptional: true));
            services.AddScoped<IRecommendationConsumer>(_ => new TrackingConsumer("next-consumer", 20, 1));
        });

        using var scope = provider.CreateScope();

        var pipeline = scope.ServiceProvider.GetRequiredService<RecommendationPipeline>();
        var context = CreateContext();

        var session = pipeline.CreateSession(context);
        var bundle = pipeline.BuildBundle(context, session);

        Assert.Equal(RecommendationBundleStatus.Finalized, bundle.Status);
    }

    private static ServiceProvider BuildProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        services.AddPropertyBusinessCapabilityRuntime();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static BusinessContextModel CreateContext()
    {
        var metadata = new BusinessContextMetadata(
            createdAtUtc: DateTime.UtcNow,
            effectiveDateUtc: new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc),
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

            return
            [
            RecommendationModel.CreateDraft(
                        RecommendationId.New(),
                        RecommendationType.Create("generic"),
                        RecommendationPriority.Create(10),
                        RecommendationConfidence.Create(0.8m),
                        new RecommendationEvidence("EVID-1", "Runtime evidence"),
                        new RecommendationExplanation("Runtime explanation"),
                        new RecommendationMetadata(
                            createdAtUtc: DateTime.UtcNow,
                            effectiveDateUtc: context.Metadata.EffectiveDateUtc,
                            version: context.Version.ToString(),
                            source: Name))
                    .MarkProposed(DateTime.UtcNow)
            ];
        }
    }

    private sealed class PassThroughDecisionHandler : IDecisionHandler
    {
        public string Name => "pass-through";

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
        public TrackingConsumer(string name, int order, int priority)
        {
            Name = name;
            Order = order;
            Priority = priority;
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => true;

        public RecommendationConsumerResult Consume(RecommendationConsumerExecutionContext context)
        {
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
}
