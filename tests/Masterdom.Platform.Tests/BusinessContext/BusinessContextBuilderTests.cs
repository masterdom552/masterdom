using Masterdom.Platform.BusinessContext;

namespace Masterdom.Platform.Tests.BusinessContext;

public sealed class BusinessContextBuilderTests
{
    [Fact]
    public void Build_ShouldComposeProviders_ByOrderThenPriority()
    {
        var registry = new BusinessContextBuilderRegistry(
        [
            new StubProvider("billing", order: 20, priority: 5),
            new StubProvider("property", order: 10, priority: 1),
            new StubProvider("metering", order: 20, priority: 10)
        ]);

        var builder = new BusinessContextBuilder(registry);

        var result = builder.Build(CreateRequest());

        Assert.Equal(["property", "metering", "billing"], result.Context.Metadata.ProviderExecutionOrder);
        Assert.Equal(3, result.Context.Snapshots.Count);
        Assert.True(result.Context.TryGetSnapshot("property.snapshot", out _));
        Assert.True(result.Context.TryGetSnapshot("metering.snapshot", out _));
        Assert.True(result.Context.TryGetSnapshot("billing.snapshot", out _));
    }

    [Fact]
    public void Build_WhenOptionalProviderFails_ShouldContinueWithWarnings()
    {
        var registry = new BusinessContextBuilderRegistry(
        [
            new StubProvider("property", order: 10, priority: 1),
            new FailingProvider("reporting", order: 30, priority: 1, isOptional: true)
        ]);

        var builder = new BusinessContextBuilder(registry);

        var result = builder.Build(CreateRequest());

        Assert.Single(result.Context.Snapshots);
        Assert.Single(result.Warnings);
        Assert.Contains("Optional provider 'reporting' failed", result.Warnings[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Build_WhenRequestContainsEffectiveDateAndVersion_ShouldPropagateMetadata()
    {
        var effectiveDate = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var request = new BusinessContextRequest(
            effectiveDateUtc: effectiveDate,
            configurationVersion: "cfg-v42",
            language: "en-US",
            securityContext: "superuser",
            userId: "u-100",
            portfolioId: "p-200",
            attributes: new Dictionary<string, string> { ["scenario"] = "runtime-composition" });

        var registry = new BusinessContextBuilderRegistry([new StubProvider("property", order: 10, priority: 1)]);
        var options = new BusinessContextOptions { Version = new BusinessContextVersion(5) };
        var builder = new BusinessContextBuilder(registry, options);

        var result = builder.Build(request);

        Assert.Equal(effectiveDate, result.Context.Metadata.EffectiveDateUtc);
        Assert.Equal("cfg-v42", result.Context.Metadata.ConfigurationVersion);
        Assert.Equal("en-US", result.Context.Metadata.Language);
        Assert.Equal("superuser", result.Context.Metadata.SecurityContext);
        Assert.Equal("u-100", result.Context.Metadata.UserId);
        Assert.Equal("p-200", result.Context.Metadata.PortfolioId);
        Assert.Equal("runtime-composition", result.Context.Metadata.Attributes["scenario"]);
        Assert.Equal(5, result.Context.Version.Value);
    }

    [Fact]
    public void Build_ShouldCreateImmutableSnapshotCollection()
    {
        var registry = new BusinessContextBuilderRegistry([new StubProvider("property", order: 10, priority: 1)]);
        var builder = new BusinessContextBuilder(registry);

        var result = builder.Build(CreateRequest());

        var dictionary = Assert.IsAssignableFrom<IDictionary<string, BusinessContextSnapshot>>(result.Context.Snapshots);

        Assert.Throws<NotSupportedException>(() =>
            dictionary.Add("other", new BusinessContextSnapshot("other", new { })));
    }

    [Fact]
    public void Build_WhenNoProvidersAreRegistered_ShouldSucceedWithEmptyContext()
    {
        var registry = new BusinessContextBuilderRegistry();
        var builder = new BusinessContextBuilder(registry);

        var result = builder.Build(CreateRequest());

        Assert.Empty(result.Context.Snapshots);
        Assert.Empty(result.Context.References);
        Assert.Empty(result.Context.Metadata.ProviderExecutionOrder);
    }

    [Fact]
    public void Build_ShouldPropagateContextReferences()
    {
        var reference = new BusinessContextReference(
            provider: "tenancy",
            source: "Tenancy",
            referenceId: "TEN-1",
            sourceVersion: "v7",
            effectiveDateUtc: new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Utc));

        var provider = new StubProvider(
            "tenancy",
            order: 10,
            priority: 1,
            references: [reference]);

        var builder = new BusinessContextBuilder(new BusinessContextBuilderRegistry([provider]));

        var result = builder.Build(CreateRequest());

        var propagated = Assert.Single(result.Context.References);
        Assert.Equal("tenancy", propagated.Provider);
        Assert.Equal("TEN-1", propagated.ReferenceId);
        Assert.Equal("v7", propagated.SourceVersion);
    }

    private static BusinessContextRequest CreateRequest()
    {
        return new BusinessContextRequest(
            effectiveDateUtc: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            configurationVersion: "cfg-v1");
    }

    private sealed class StubProvider : IBusinessContextProvider
    {
        private readonly IReadOnlyList<BusinessContextReference> _references;

        public StubProvider(
            string name,
            int order,
            int priority,
            IReadOnlyList<BusinessContextReference>? references = null)
        {
            Name = name;
            Order = order;
            Priority = priority;
            _references = references ?? [];
        }

        public string Name { get; }

        public int Order { get; }

        public int Priority { get; }

        public bool IsOptional => true;

        public BusinessContextProviderResult Provide(BusinessContextRequest request)
        {
            return new BusinessContextProviderResult(
                snapshots:
                [
                    new BusinessContextSnapshot(
                        Key: $"{Name}.snapshot",
                        Payload: new { request.EffectiveDateUtc, request.ConfigurationVersion })
                ],
                references: _references,
                metadata: new Dictionary<string, string> { ["version"] = "1" });
        }
    }

    private sealed class FailingProvider : IBusinessContextProvider
    {
        private readonly bool _isOptional;

        public FailingProvider(string name, int order, int priority, bool isOptional)
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

        public BusinessContextProviderResult Provide(BusinessContextRequest request)
        {
            _ = request;
            throw new InvalidOperationException("simulated failure");
        }
    }
}
