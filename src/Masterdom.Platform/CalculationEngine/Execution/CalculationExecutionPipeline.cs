using System.Collections.Immutable;
using System.Diagnostics;
using Masterdom.Platform.CalculationEngine.Contracts;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal sealed class CalculationOperationExecutionDefinition
{
    public CalculationOperationExecutionDefinition(
        ICalculationOperation operation,
        ICalculationOperationDescriptor descriptor)
    {
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
    }

    public ICalculationOperation Operation { get; }

    public ICalculationOperationDescriptor Descriptor { get; }
}

internal interface ICalculationExecutionRegistry : ICalculationRegistry
{
    bool TryResolveDefinition(
        CalculationOperationDescriptorId operationId,
        out CalculationOperationExecutionDefinition definition);

    bool TryResolveDefinition(
        CalculationOperationCapabilityId capabilityId,
        out CalculationOperationExecutionDefinition definition);
}

internal sealed class CalculationExecutionRegistry : ICalculationExecutionRegistry
{
    private readonly ImmutableDictionary<string, CalculationOperationExecutionDefinition> _byOperationId;
    private readonly ImmutableDictionary<string, CalculationOperationExecutionDefinition> _byCapabilityId;

    public CalculationExecutionRegistry(IEnumerable<CalculationOperationExecutionDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var definitionList = definitions.ToArray();

        _byOperationId = definitionList.ToImmutableDictionary(
            definition => definition.Descriptor.DescriptorId.Value,
            definition => definition,
            StringComparer.OrdinalIgnoreCase);

        _byCapabilityId = definitionList.ToImmutableDictionary(
            definition => definition.Descriptor.CapabilityId.Value,
            definition => definition,
            StringComparer.OrdinalIgnoreCase);
    }

    public bool TryResolve(CalculationOperationDescriptorId operationId, out ICalculationOperation operation)
    {
        if (operationId is null)
        {
            throw new ArgumentNullException(nameof(operationId));
        }

        if (TryResolveDefinition(operationId, out var definition))
        {
            operation = definition.Operation;
            return true;
        }

        operation = default!;
        return false;
    }

    public bool TryResolve(CalculationOperationCapabilityId capabilityId, out ICalculationOperation operation)
    {
        if (capabilityId is null)
        {
            throw new ArgumentNullException(nameof(capabilityId));
        }

        if (TryResolveDefinition(capabilityId, out var definition))
        {
            operation = definition.Operation;
            return true;
        }

        operation = default!;
        return false;
    }

    public bool TryResolveDefinition(
        CalculationOperationDescriptorId operationId,
        out CalculationOperationExecutionDefinition definition)
    {
        if (operationId is null)
        {
            throw new ArgumentNullException(nameof(operationId));
        }

        return _byOperationId.TryGetValue(operationId.Value, out definition!);
    }

    public bool TryResolveDefinition(
        CalculationOperationCapabilityId capabilityId,
        out CalculationOperationExecutionDefinition definition)
    {
        if (capabilityId is null)
        {
            throw new ArgumentNullException(nameof(capabilityId));
        }

        return _byCapabilityId.TryGetValue(capabilityId.Value, out definition!);
    }
}

internal sealed class CalculationRequestValidator
{
    public void Validate(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.OperationId);
        ArgumentNullException.ThrowIfNull(request.Context);
        ArgumentNullException.ThrowIfNull(request.Input);

        if (string.IsNullOrWhiteSpace(request.OperationId.Value))
        {
            throw new CalculationOperationValidationException("OperationId is required.");
        }

        if (request.Context.EffectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new CalculationOperationValidationException("CalculationContext.EffectiveDateUtc must be UTC.");
        }

        if (request.Input.Values is null)
        {
            throw new CalculationOperationValidationException("CalculationInput values are required.");
        }
    }
}

internal sealed class CalculationOperationResolver
{
    private readonly ICalculationExecutionRegistry _registry;

    public CalculationOperationResolver(ICalculationExecutionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public CalculationOperationExecutionDefinition Resolve(ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_registry.TryResolveDefinition(request.OperationId, out var definition))
        {
            return definition;
        }

        throw new CalculationOperationValidationException(
            $"Calculation operation '{request.OperationId.Value}' was not found.");
    }
}

internal sealed class CalculationExecutor
{
    public CalculationOutput Execute(
        CalculationOperationExecutionDefinition definition,
        ICalculationRequest request)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(request);

        var result = definition.Operation.Execute(request);

        if (result is null)
        {
            throw new CalculationOperationValidationException(
                $"Calculation operation '{definition.Descriptor.OperationName}' returned no result.");
        }

        if (result.Output is null)
        {
            throw new CalculationOperationValidationException(
                $"Calculation operation '{definition.Descriptor.OperationName}' returned no output.");
        }

        return new CalculationOutput(result.Output.Values);
    }
}

internal sealed class CalculationResultValidator
{
    public void Validate(
        ICalculationResult result,
        CalculationOperationExecutionDefinition definition,
        CalculationExecutionMetadata executionMetadata)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(executionMetadata);

        if (result.Output is null)
        {
            throw new CalculationOperationValidationException(
                $"Calculation operation '{definition.Descriptor.OperationName}' produced a null output.");
        }

        if (result.Output.Values is null)
        {
            throw new CalculationOperationValidationException(
                $"Calculation operation '{definition.Descriptor.OperationName}' produced invalid output values.");
        }

        if (!string.Equals(executionMetadata.OperationId.Value, definition.Descriptor.DescriptorId.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata operation id mismatch for '{definition.Descriptor.OperationName}'.");
        }

        if (!string.Equals(executionMetadata.CapabilityId.Value, definition.Descriptor.CapabilityId.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata capability id mismatch for '{definition.Descriptor.OperationName}'.");
        }

        if (executionMetadata.CapabilityCategory != definition.Descriptor.CapabilityCategory)
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata capability category mismatch for '{definition.Descriptor.OperationName}'.");
        }

        if (executionMetadata.CompatibilityStatus != definition.Descriptor.CompatibilityStatus)
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata compatibility status mismatch for '{definition.Descriptor.OperationName}'.");
        }

        if (!string.Equals(executionMetadata.DescriptorVersion.Value, definition.Descriptor.OperationVersion.Value, StringComparison.OrdinalIgnoreCase))
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata descriptor version mismatch for '{definition.Descriptor.OperationName}'.");
        }

        if (executionMetadata.ExecutionTimestampUtc.Kind != DateTimeKind.Utc)
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata timestamp must be UTC for '{definition.Descriptor.OperationName}'.");
        }

        if (executionMetadata.ExecutionDuration < TimeSpan.Zero)
        {
            throw new CalculationOperationValidationException(
                $"Execution metadata duration must not be negative for '{definition.Descriptor.OperationName}'.");
        }
    }
}

internal sealed class CalculationExecutionPipeline : ICalculationEngine
{
    private readonly CalculationRequestValidator _requestValidator;
    private readonly CalculationOperationResolver _operationResolver;
    private readonly CalculationExecutor _executor;
    private readonly CalculationResultValidator _resultValidator;
    private readonly CalculationExecutionPipelineDescriptor _descriptor;

    public CalculationExecutionPipeline(
        CalculationRequestValidator requestValidator,
        CalculationOperationResolver operationResolver,
        CalculationExecutor executor,
        CalculationResultValidator resultValidator)
    {
        _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        _operationResolver = operationResolver ?? throw new ArgumentNullException(nameof(operationResolver));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _resultValidator = resultValidator ?? throw new ArgumentNullException(nameof(resultValidator));
        _descriptor = CalculationExecutionPipelineMetadata.Descriptor;
    }

    public ICalculationResult Execute(ICalculationRequest request)
    {
        _requestValidator.Validate(request);

        var definition = _operationResolver.Resolve(request);
        var startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var output = _executor.Execute(definition, request);

        stopwatch.Stop();
        var completedAtUtc = DateTime.UtcNow;
        var executedStages = new[]
        {
            CalculationExecutionStageIdentifiers.InputValidation,
            CalculationExecutionStageIdentifiers.OperationResolution,
            CalculationExecutionStageIdentifiers.OperationExecution,
            CalculationExecutionStageIdentifiers.OutputValidation,
            CalculationExecutionStageIdentifiers.MetadataCapture
        };

        var executionMetadata = new CalculationExecutionMetadata(
            definition.Descriptor.DescriptorId,
            definition.Descriptor.OperationVersion,
            startedAtUtc,
            stopwatch.Elapsed,
            definition.Descriptor.CapabilityId,
            definition.Descriptor.CapabilityCategory,
            definition.Descriptor.CompatibilityStatus);

        var executionRecord = new CalculationExecutionRecord(
            Guid.CreateVersion7(),
            _descriptor.CapabilityId,
            _descriptor.PipelineId,
            _descriptor.PipelineVersion,
            _descriptor.SupportedContractVersion,
            _descriptor.DescriptorVersion,
            startedAtUtc,
            completedAtUtc,
            stopwatch.Elapsed,
            executedStages,
            CalculationExecutionRecordStatus.Succeeded);

        var result = new CalculationResult(output, executionMetadata);

        _resultValidator.Validate(result, definition, executionMetadata);

        _ = executionRecord;

        return result;
    }
}
