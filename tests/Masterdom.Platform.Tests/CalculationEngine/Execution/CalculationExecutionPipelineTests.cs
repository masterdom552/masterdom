using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Execution;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.Tests.CalculationEngine.Execution;

public sealed class CalculationExecutionPipelineTests
{
    [Fact]
    public void Execute_ShouldUseSingleResolutionAndSingleExecutionPath()
    {
        var operation = new StubOperation();
        var definition = BuildDefinition(operation);
        var registry = new GuardedRegistry(definition);
        var pipeline = BuildPipeline(registry);

        var request = new CalculationRequest(
            definition.Descriptor.DescriptorId,
            new CalculationContext(DateTime.SpecifyKind(new DateTime(2026, 8, 4), DateTimeKind.Utc)),
            new CalculationInput(new Dictionary<string, object?> { ["value"] = 42 }));

        var result = pipeline.Execute(request);

        Assert.Equal(1, operation.ExecutionCount);
        Assert.Equal(definition.Descriptor.DescriptorId.Value, result.ExecutionMetadata.OperationId.Value);
        Assert.Equal(definition.Descriptor.CapabilityId.Value, result.ExecutionMetadata.CapabilityId.Value);
        Assert.Equal(definition.Descriptor.CapabilityCategory, result.ExecutionMetadata.CapabilityCategory);
        Assert.Equal(definition.Descriptor.CompatibilityStatus, result.ExecutionMetadata.CompatibilityStatus);
        Assert.Equal(definition.Descriptor.OperationVersion.Value, result.ExecutionMetadata.DescriptorVersion.Value);
        Assert.Equal(42, result.Output.Values["value"]);
    }

    [Fact]
    public void Execute_ShouldNotBypassOperationResolutionByCapabilityLookup()
    {
        var operation = new StubOperation();
        var definition = BuildDefinition(operation);
        var registry = new ThrowingCapabilityRegistry(definition);
        var pipeline = BuildPipeline(registry);

        var request = new CalculationRequest(
            definition.Descriptor.DescriptorId,
            new CalculationContext(DateTime.SpecifyKind(new DateTime(2026, 8, 4), DateTimeKind.Utc)),
            new CalculationInput(new Dictionary<string, object?> { ["value"] = 7 }));

        var result = pipeline.Execute(request);

        Assert.Equal(1, operation.ExecutionCount);
        Assert.Equal(7, result.Output.Values["value"]);
    }

    [Fact]
    public void PipelineComponents_ShouldRemainInternal()
    {
        var executionTypes = typeof(CalculationExecutionPipeline).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "Masterdom.Platform.CalculationEngine.Execution")
            .ToArray();

        Assert.NotEmpty(executionTypes);
        Assert.All(executionTypes, type => Assert.False(type.IsPublic));
    }

    [Fact]
    public void PipelineDescriptor_ShouldBeImmutableAndVersioned()
    {
        var descriptor = CalculationExecutionPipelineMetadata.Descriptor;

        Assert.Equal("execution.pipeline", descriptor.CapabilityId);
        Assert.Equal("calculation.execution.pipeline", descriptor.PipelineId);
        Assert.Equal("1.0", descriptor.PipelineVersion);
        Assert.Equal("1.0", descriptor.SupportedContractVersion);
        Assert.Equal("1.0", descriptor.DescriptorVersion);
        Assert.Equal(["validation.input", "validation.output"], descriptor.ValidationStages);
        Assert.Equal(["resolution.operation", "execution.operation"], descriptor.ExecutionStages);
        Assert.Equal(["metadata.capture"], descriptor.MetadataStages);
    }

    [Fact]
    public void ExecutionRecord_ShouldCaptureStableStageIdentifiers()
    {
        var startedAtUtc = new DateTime(2026, 8, 4, 12, 0, 0, DateTimeKind.Utc);
        var completedAtUtc = startedAtUtc.AddMilliseconds(10);

        var record = new CalculationExecutionRecord(
            Guid.CreateVersion7(),
            "execution.pipeline",
            "calculation.execution.pipeline",
            "1.0",
            "1.0",
            "1.0",
            startedAtUtc,
            completedAtUtc,
            TimeSpan.FromMilliseconds(10),
            [
                CalculationExecutionStageIdentifiers.InputValidation,
                CalculationExecutionStageIdentifiers.OperationResolution,
                CalculationExecutionStageIdentifiers.OperationExecution,
                CalculationExecutionStageIdentifiers.OutputValidation,
                CalculationExecutionStageIdentifiers.MetadataCapture
            ],
            CalculationExecutionRecordStatus.Succeeded);

        Assert.Equal("execution.pipeline", record.CapabilityId);
        Assert.Equal("calculation.execution.pipeline", record.PipelineId);
        Assert.Equal("1.0", record.PipelineVersion);
        Assert.Equal("1.0", record.ContractVersion);
        Assert.Equal("1.0", record.MetadataVersion);
        Assert.Equal(startedAtUtc, record.StartedAt);
        Assert.Equal(completedAtUtc, record.CompletedAt);
        Assert.Equal(TimeSpan.FromMilliseconds(10), record.Duration);
        Assert.Equal(CalculationExecutionRecordStatus.Succeeded, record.ExecutionStatus);
        Assert.Equal(["validation.input", "resolution.operation", "execution.operation", "validation.output", "metadata.capture"], record.ExecutedStages);
    }

    [Fact]
    public void StageIdentifiers_ShouldRemainStable()
    {
        Assert.Equal("validation.input", CalculationExecutionStageIdentifiers.InputValidation);
        Assert.Equal("resolution.operation", CalculationExecutionStageIdentifiers.OperationResolution);
        Assert.Equal("execution.operation", CalculationExecutionStageIdentifiers.OperationExecution);
        Assert.Equal("validation.output", CalculationExecutionStageIdentifiers.OutputValidation);
        Assert.Equal("metadata.capture", CalculationExecutionStageIdentifiers.MetadataCapture);
    }

    [Fact]
    public void Pipeline_ShouldRejectNonUtcContext()
    {
        var operation = new StubOperation();
        var definition = BuildDefinition(operation);
        var registry = new GuardedRegistry(definition);
        var pipeline = BuildPipeline(registry);

        var request = new CalculationRequest(
            definition.Descriptor.DescriptorId,
            new StubContext(new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Local)),
            new CalculationInput(new Dictionary<string, object?>()));

        Assert.Throws<CalculationOperationValidationException>(() => pipeline.Execute(request));
    }

    [Fact]
    public void Pipeline_ShouldRejectOperationResult_WithNullOutput()
    {
        var definition = BuildDefinition(new NullOutputOperation());
        var registry = new GuardedRegistry(definition);
        var pipeline = BuildPipeline(registry);

        var request = new CalculationRequest(
            definition.Descriptor.DescriptorId,
            new CalculationContext(DateTime.SpecifyKind(new DateTime(2026, 8, 4), DateTimeKind.Utc)),
            new CalculationInput(new Dictionary<string, object?> { ["value"] = 1 }));

        Assert.Throws<CalculationOperationValidationException>(() => pipeline.Execute(request));
    }

    private static CalculationExecutionPipeline BuildPipeline(ICalculationExecutionRegistry registry)
    {
        return new CalculationExecutionPipeline(
            new CalculationRequestValidator(),
            new CalculationOperationResolver(registry),
            new CalculationExecutor(),
            new CalculationResultValidator());
    }

    private static CalculationOperationExecutionDefinition BuildDefinition(ICalculationOperation operation)
    {
        var descriptor = new CalculationOperationDescriptor
        {
            DescriptorId = CalculationOperationDescriptorId.Create("ce-op-execution-1"),
            SourceType = CalculationOperationDescriptorSourceType.Test,
            SchemaVersion = "1.0",
            OperationName = "Execution Test Operation",
            CapabilityId = CalculationOperationCapabilityId.Create("execution.test"),
            OperationVersion = CalculationOperationVersion.Create("1.0.0"),
            Description = "Test execution definition.",
            PrimitiveFamily = CalculationOperationPrimitiveFamily.Validation,
            CapabilityCategory = CalculationOperationCapabilityCategory.Validation,
            CompositionLevel = CalculationOperationCompositionLevel.Primitive,
            OperationCategory = CalculationOperationCategory.Primitive,
            ExecutionClassification = CalculationOperationExecutionClassification.Primitive,
            Purity = CalculationOperationPurity.Pure,
            Determinism = CalculationOperationDeterminism.Deterministic,
            Stability = CalculationOperationStability.Fundamental,
            CompatibilityStatus = CalculationOperationCompatibilityStatus.Supported,
            TimeComplexity = "O(1)",
            SpaceComplexity = "O(1)",
            DependencyCapabilityIds = [],
            TechnicalTags = ["execution"],
            MathematicalTags = ["validation"]
        };

        return new CalculationOperationExecutionDefinition(operation, descriptor);
    }

    private sealed class StubOperation : ICalculationOperation
    {
        public int ExecutionCount { get; private set; }

        public ICalculationResult Execute(ICalculationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ExecutionCount++;

            var output = new CalculationOutput(new Dictionary<string, object?>
            {
                ["value"] = request.Input.Values["value"]
            });

            var bogusMetadata = new CalculationExecutionMetadata(
                CalculationOperationDescriptorId.Create("ce-op-execution-1"),
                CalculationOperationVersion.Create("1.0.0"),
                DateTime.UtcNow,
                TimeSpan.Zero,
                CalculationOperationCapabilityId.Create("execution.test"),
                CalculationOperationCapabilityCategory.Validation,
                CalculationOperationCompatibilityStatus.Supported);

            return new CalculationResult(output, bogusMetadata);
        }
    }

    private sealed class NullOutputOperation : ICalculationOperation
    {
        public ICalculationResult Execute(ICalculationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new NullOutputResult();
        }
    }

    private sealed class NullOutputResult : ICalculationResult
    {
        public ICalculationOutput Output => null!;

        public ICalculationExecutionMetadata ExecutionMetadata => new CalculationExecutionMetadata(
            CalculationOperationDescriptorId.Create("ce-op-execution-1"),
            CalculationOperationVersion.Create("1.0.0"),
            DateTime.UtcNow,
            TimeSpan.Zero,
            CalculationOperationCapabilityId.Create("execution.test"),
            CalculationOperationCapabilityCategory.Validation,
            CalculationOperationCompatibilityStatus.Supported);
    }

    private sealed class StubContext : ICalculationContext
    {
        public StubContext(DateTime effectiveDateUtc)
        {
            EffectiveDateUtc = effectiveDateUtc;
        }

        public DateTime EffectiveDateUtc { get; }

        public IReadOnlyDictionary<string, string> ConfigurationSnapshots { get; } = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> StrategyIdentifiers { get; } = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> CallerMetadata { get; } = new Dictionary<string, string>();
    }

    private sealed class GuardedRegistry : ICalculationExecutionRegistry
    {
        private readonly CalculationOperationExecutionDefinition _definition;

        public GuardedRegistry(CalculationOperationExecutionDefinition definition)
        {
            _definition = definition;
        }

        public bool TryResolve(CalculationOperationDescriptorId operationId, out ICalculationOperation operation)
        {
            if (operationId.Value == _definition.Descriptor.DescriptorId.Value)
            {
                operation = _definition.Operation;
                return true;
            }

            operation = default!;
            return false;
        }

        public bool TryResolve(CalculationOperationCapabilityId capabilityId, out ICalculationOperation operation)
        {
            throw new InvalidOperationException("Capability lookup should not be used by the execution pipeline.");
        }

        public bool TryResolveDefinition(CalculationOperationDescriptorId operationId, out CalculationOperationExecutionDefinition definition)
        {
            if (operationId.Value == _definition.Descriptor.DescriptorId.Value)
            {
                definition = _definition;
                return true;
            }

            definition = default!;
            return false;
        }

        public bool TryResolveDefinition(CalculationOperationCapabilityId capabilityId, out CalculationOperationExecutionDefinition definition)
        {
            throw new InvalidOperationException("Capability lookup should not be used by the execution pipeline.");
        }
    }

    private sealed class ThrowingCapabilityRegistry : ICalculationExecutionRegistry
    {
        private readonly CalculationOperationExecutionDefinition _definition;

        public ThrowingCapabilityRegistry(CalculationOperationExecutionDefinition definition)
        {
            _definition = definition;
        }

        public bool TryResolve(CalculationOperationDescriptorId operationId, out ICalculationOperation operation)
        {
            if (operationId.Value == _definition.Descriptor.DescriptorId.Value)
            {
                operation = _definition.Operation;
                return true;
            }

            operation = default!;
            return false;
        }

        public bool TryResolve(CalculationOperationCapabilityId capabilityId, out ICalculationOperation operation)
        {
            throw new InvalidOperationException("Capability lookup should not be used by the execution pipeline.");
        }

        public bool TryResolveDefinition(CalculationOperationDescriptorId operationId, out CalculationOperationExecutionDefinition definition)
        {
            if (operationId.Value == _definition.Descriptor.DescriptorId.Value)
            {
                definition = _definition;
                return true;
            }

            definition = default!;
            return false;
        }

        public bool TryResolveDefinition(CalculationOperationCapabilityId capabilityId, out CalculationOperationExecutionDefinition definition)
        {
            throw new InvalidOperationException("Capability lookup should not be used by the execution pipeline.");
        }
    }
}
