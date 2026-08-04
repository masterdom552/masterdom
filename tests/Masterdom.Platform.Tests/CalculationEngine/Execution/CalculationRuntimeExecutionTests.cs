using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine;
using Masterdom.Platform.CalculationEngine.Execution;
using Masterdom.Platform.CalculationEngine.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Masterdom.Platform.Tests.CalculationEngine.Execution;

public sealed class CalculationRuntimeExecutionTests
{
    private static readonly DateTime EffectiveAtUtc = new(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void PublicRuntime_ShouldExecute_ByCapabilityId()
    {
        var runtime = CreateRuntime();

        var result = runtime.Execute(new CalculationRuntimeRequest(
            CalculationCapabilityId.Create("scoring.scenario"),
            new CalculationContext(EffectiveAtUtc),
            new CalculationInput(Input(
                ("componentValues", new decimal[] { 0.8m, 0.6m }),
                ("componentWeights", new decimal[] { 3m, 1m }),
                ("clampMin", 0m),
                ("clampMax", 1m)))));

        Assert.Equal("scoring.scenario", result.ExecutionMetadata.CapabilityId.Value);
        Assert.Equal(0.75m, Assert.IsType<decimal>(result.Output.Values["compositeScenarioScore"]));
    }

    [Fact]
    public void AddCalculationEngine_ShouldRegister_PublicRuntimeGateway()
    {
        var services = new ServiceCollection();

        services.AddCalculationEngine();

        using var provider = services.BuildServiceProvider();
        var runtime = provider.GetService<ICalculationRuntime>();

        Assert.NotNull(runtime);
    }

    [Fact]
    public void AddCalculationEngine_ShouldRegister_RuntimeAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddCalculationEngine();

        using var provider = services.BuildServiceProvider();
        using var scopeA = provider.CreateScope();
        using var scopeB = provider.CreateScope();

        var runtimeA = scopeA.ServiceProvider.GetRequiredService<ICalculationRuntime>();
        var runtimeB = scopeB.ServiceProvider.GetRequiredService<ICalculationRuntime>();

        Assert.Same(runtimeA, runtimeB);
    }

    [Fact]
    public void AddCalculationEngine_ShouldNotExpose_InternalExecutionServices()
    {
        var services = new ServiceCollection();
        services.AddCalculationEngine();

        using var provider = services.BuildServiceProvider();

        Assert.Null(provider.GetService<ICalculationEngine>());

        var executionRuntimeType = typeof(ICalculationRuntime).Assembly.GetType(
            "Masterdom.Platform.CalculationEngine.Execution.CalculationRuntime",
            throwOnError: true)!;

        Assert.Null(provider.GetService(executionRuntimeType));
    }

    [Fact]
    public void Runtime_ShouldResolveCapabilityInternally_AndInvokeEngineOnce()
    {
        var metadataRegistry = new CalculationOperationRegistry();
        var spy = new CountingEngine(CalculationEngineFactory.CreateDefault());
        var runtime = new CalculationRuntime(spy, metadataRegistry);

        var request = new CalculationRuntimeRequest(
            CalculationCapabilityId.Create("scoring.scenario"),
            new CalculationContext(EffectiveAtUtc),
            new CalculationInput(Input(
                ("componentValues", new decimal[] { 0.8m, 0.6m }),
                ("componentWeights", new decimal[] { 3m, 1m }),
                ("clampMin", 0m),
                ("clampMax", 1m))));

        var result = runtime.Execute(request);
        var descriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("scoring.scenario"));

        Assert.Equal(1, spy.ExecuteCount);
        Assert.NotNull(spy.LastRequest);
        Assert.Equal(descriptor.DescriptorId.Value, spy.LastRequest!.OperationId.Value);
        Assert.Equal(descriptor.DescriptorId.Value, result.ExecutionMetadata.OperationId.Value);
    }

    [Fact]
    public void RuntimeRegistry_ShouldResolve_PrimitivesAndComposites_ByCapabilityId_AndDescriptorId()
    {
        var registry = CalculationRuntimeExecutionRegistryBuilder.CreateDefault();
        var metadataRegistry = new CalculationOperationRegistry();
        var primitiveDescriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("aggregation.sum"));
        var compositeDescriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("scoring.scenario"));

        Assert.True(registry.TryResolve(primitiveDescriptor.DescriptorId, out var primitiveByDescriptor));
        Assert.True(registry.TryResolve(primitiveDescriptor.CapabilityId, out var primitiveByCapability));
        Assert.True(registry.TryResolve(compositeDescriptor.DescriptorId, out var compositeByDescriptor));
        Assert.True(registry.TryResolve(compositeDescriptor.CapabilityId, out var compositeByCapability));

        Assert.NotNull(primitiveByDescriptor);
        Assert.NotNull(primitiveByCapability);
        Assert.NotNull(compositeByDescriptor);
        Assert.NotNull(compositeByCapability);
    }

    [Fact]
    public void RuntimeRegistry_ShouldReturnFalse_ForUnknownCapabilityId()
    {
        var registry = CalculationRuntimeExecutionRegistryBuilder.CreateDefault();

        var resolved = registry.TryResolve(CalculationOperationCapabilityId.Create("runtime.unknown"), out var operation);

        Assert.False(resolved);
        Assert.Null(operation);
    }

    [Theory]
    [MemberData(nameof(CompositeRuntimeCases))]
    public void DefaultRuntime_ShouldExecute_FrozenCompositeDescriptors(
        string capabilityId,
        IReadOnlyDictionary<string, object?> input,
        string expectedKey,
        object expectedValue)
    {
        var engine = CalculationEngineFactory.CreateDefault();
        var metadataRegistry = new CalculationOperationRegistry();
        var descriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create(capabilityId));

        var request = new CalculationRequest(
            descriptor.DescriptorId,
            new CalculationContext(EffectiveAtUtc),
            new CalculationInput(new Dictionary<string, object?>(input, StringComparer.OrdinalIgnoreCase)));

        var result = engine.Execute(request);

        Assert.Equal(descriptor.DescriptorId.Value, result.ExecutionMetadata.OperationId.Value);
        Assert.Equal(descriptor.CapabilityId.Value, result.ExecutionMetadata.CapabilityId.Value);
        Assert.Equal(descriptor.CapabilityCategory, result.ExecutionMetadata.CapabilityCategory);
        Assert.Equal(descriptor.CompatibilityStatus, result.ExecutionMetadata.CompatibilityStatus);
        Assert.Equal(descriptor.OperationVersion.Value, result.ExecutionMetadata.DescriptorVersion.Value);
        Assert.True(result.Output.Values.ContainsKey(expectedKey));

        AssertRuntimeValue(result.Output.Values[expectedKey], expectedValue);
    }

    [Fact]
    public void DefaultRuntime_ShouldRemainDeterministic_AcrossRepeatedCompositeExecution()
    {
        var engine = CalculationEngineFactory.CreateDefault();
        var metadataRegistry = new CalculationOperationRegistry();
        var descriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("scoring.scenario"));
        var input = Input(
            ("componentValues", new decimal[] { 0.8m, 0.6m }),
            ("componentWeights", new decimal[] { 3m, 1m }),
            ("clampMin", 0m),
            ("clampMax", 1m));

        var request = new CalculationRequest(
            descriptor.DescriptorId,
            new CalculationContext(EffectiveAtUtc),
            new CalculationInput(input));

        var first = engine.Execute(request);
        var second = engine.Execute(request);

        Assert.Equal(first.ExecutionMetadata.CapabilityId.Value, second.ExecutionMetadata.CapabilityId.Value);
        Assert.Equal(first.Output.Values["compositeScenarioScore"], second.Output.Values["compositeScenarioScore"]);
    }

    [Fact]
    public void DefaultRuntime_ShouldReject_InvalidCompositeInput()
    {
        var engine = CalculationEngineFactory.CreateDefault();
        var metadataRegistry = new CalculationOperationRegistry();
        var descriptor = metadataRegistry.ResolveByCapabilityId(CalculationOperationCapabilityId.Create("validation.pagination"));

        var request = new CalculationRequest(
            descriptor.DescriptorId,
            new CalculationContext(EffectiveAtUtc),
            new CalculationInput(Input(
                ("requestedPage", 10m),
                ("minimumPage", 1m),
                ("maximumPage", 5m),
                ("currentItemCount", 20m),
                ("totalItemCount", 80m))));

        Assert.Throws<CalculationOperationValidationException>(() => engine.Execute(request));
    }

    public static IEnumerable<object[]> CompositeRuntimeCases()
    {
        yield return [
            "estimation.consumption",
            Input(
                ("historicalValues", new decimal[] { 100m, 110m, 90m }),
                ("historicalWeights", new decimal[] { 1m, 2m, 1m }),
                ("blendWeight", 0.25m),
                ("occupancyNumerator", 9m),
                ("occupancyDenominator", 10m),
                ("completenessObservedCount", 8m),
                ("completenessExpectedCount", 10m),
                ("clampMin", 0m),
                ("clampMax", 200m)),
            "dataCompletenessRatio",
            0.8m
        ];

        yield return [
            "forecast.projection",
            Input(
                ("baselineConsumption", 120m),
                ("currentObservedConsumption", 108m),
                ("previousObservedConsumption", 90m),
                ("threshold", 120m)),
            "projectedConsumption",
            144m
        ];

        yield return [
            "scoring.confidence_composite",
            Input(
                ("observedValues", new decimal[] { 10m, 14m, 13m }),
                ("spreadUpperBound", 8m),
                ("minConfidence", 0m),
                ("maxConfidence", 1m)),
            "confidenceScore",
            0m
        ];

        yield return [
            "scoring.scenario",
            Input(
                ("componentValues", new decimal[] { 0.8m, 0.6m }),
                ("componentWeights", new decimal[] { 3m, 1m }),
                ("clampMin", 0m),
                ("clampMax", 1m)),
            "compositeScenarioScore",
            0.75m
        ];

        yield return [
            "ranking.scenario",
            Input(
                ("primaryScores", new decimal[] { 0.9m, 0.9m, 0.7m }),
                ("secondaryScores", new decimal[] { 0.2m, 0.8m, 0.1m }),
                ("topCount", 2)),
            "rankedScenarioCollection",
            new[] { 1, 0 }
        ];

        yield return [
            "transformation.import_canonical",
            Input(
                ("rawDate", "2026-08-04"),
                ("rawNumber", "42.5000"),
                ("rawBoolean", "TRUE"),
                ("numberRangeMin", 0m),
                ("numberRangeMax", 100m),
                ("inclusiveMin", true),
                ("inclusiveMax", true)),
            "canonicalNumber",
            "42.5"
        ];

        yield return [
            "validation.pagination",
            Input(
                ("requestedPage", 10m),
                ("minimumPage", 1m),
                ("maximumPage", 5m),
                ("currentItemCount", 20m),
                ("totalItemCount", 80m),
                ("pageSize", 20m)),
            "totalPageCount",
            4
        ];
    }

    private static Dictionary<string, object?> Input(params (string key, object? value)[] values)
    {
        return values.ToDictionary(pair => pair.key, pair => pair.value, StringComparer.OrdinalIgnoreCase);
    }

    private static ICalculationRuntime CreateRuntime()
    {
        var services = new ServiceCollection();
        services.AddCalculationEngine();

        using var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ICalculationRuntime>();
    }

    private static void AssertRuntimeValue(object? actual, object expected)
    {
        if (expected is decimal expectedDecimal)
        {
            Assert.IsType<decimal>(actual);
            Assert.Equal(expectedDecimal, (decimal)actual!);
            return;
        }

        if (expected is int expectedInt)
        {
            Assert.IsType<int>(actual);
            Assert.Equal(expectedInt, (int)actual!);
            return;
        }

        if (expected is int[] expectedIntArray)
        {
            var actualIntArray = Assert.IsAssignableFrom<IEnumerable<int>>(actual).ToArray();
            Assert.Equal(expectedIntArray, actualIntArray);
            return;
        }

        Assert.Equal(expected, actual);
    }

    private sealed class CountingEngine : ICalculationEngine
    {
        private readonly ICalculationEngine _inner;

        public CountingEngine(ICalculationEngine inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public int ExecuteCount { get; private set; }

        public ICalculationRequest? LastRequest { get; private set; }

        public ICalculationResult Execute(ICalculationRequest request)
        {
            LastRequest = request;
            ExecuteCount++;
            return _inner.Execute(request);
        }
    }
}
