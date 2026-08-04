using System.Collections.Immutable;
using Masterdom.Platform.CalculationEngine.Metadata;

namespace Masterdom.Platform.CalculationEngine.Contracts;

public interface ICalculationInput
{
    IReadOnlyDictionary<string, object?> Values { get; }
}

public sealed class CalculationInput : ICalculationInput
{
    private readonly ImmutableDictionary<string, object?> _values;

    public CalculationInput(IReadOnlyDictionary<string, object?>? values = null)
    {
        _values = values is null
            ? ImmutableDictionary<string, object?>.Empty
            : ImmutableDictionary.CreateRange(values);
    }

    public IReadOnlyDictionary<string, object?> Values => _values;
}

public interface ICalculationOutput
{
    IReadOnlyDictionary<string, object?> Values { get; }
}

public sealed class CalculationOutput : ICalculationOutput
{
    private readonly ImmutableDictionary<string, object?> _values;

    public CalculationOutput(IReadOnlyDictionary<string, object?>? values = null)
    {
        _values = values is null
            ? ImmutableDictionary<string, object?>.Empty
            : ImmutableDictionary.CreateRange(values);
    }

    public IReadOnlyDictionary<string, object?> Values => _values;
}

public interface ICalculationContext
{
    DateTime EffectiveDateUtc { get; }

    IReadOnlyDictionary<string, string> ConfigurationSnapshots { get; }

    IReadOnlyDictionary<string, string> StrategyIdentifiers { get; }

    IReadOnlyDictionary<string, string> CallerMetadata { get; }
}

public sealed class CalculationContext : ICalculationContext
{
    private readonly ImmutableDictionary<string, string> _configurationSnapshots;
    private readonly ImmutableDictionary<string, string> _strategyIdentifiers;
    private readonly ImmutableDictionary<string, string> _callerMetadata;

    public CalculationContext(
        DateTime effectiveDateUtc,
        IReadOnlyDictionary<string, string>? configurationSnapshots = null,
        IReadOnlyDictionary<string, string>? strategyIdentifiers = null,
        IReadOnlyDictionary<string, string>? callerMetadata = null)
    {
        if (effectiveDateUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("EffectiveDateUtc must be UTC.");
        }

        EffectiveDateUtc = effectiveDateUtc;
        _configurationSnapshots = configurationSnapshots is null
            ? ImmutableDictionary<string, string>.Empty
            : ImmutableDictionary.CreateRange(configurationSnapshots);
        _strategyIdentifiers = strategyIdentifiers is null
            ? ImmutableDictionary<string, string>.Empty
            : ImmutableDictionary.CreateRange(strategyIdentifiers);
        _callerMetadata = callerMetadata is null
            ? ImmutableDictionary<string, string>.Empty
            : ImmutableDictionary.CreateRange(callerMetadata);
    }

    public DateTime EffectiveDateUtc { get; }

    public IReadOnlyDictionary<string, string> ConfigurationSnapshots => _configurationSnapshots;

    public IReadOnlyDictionary<string, string> StrategyIdentifiers => _strategyIdentifiers;

    public IReadOnlyDictionary<string, string> CallerMetadata => _callerMetadata;
}

public interface ICalculationRequest
{
    CalculationOperationDescriptorId OperationId { get; }

    ICalculationContext Context { get; }

    ICalculationInput Input { get; }
}

public sealed class CalculationRequest : ICalculationRequest
{
    public CalculationRequest(
        CalculationOperationDescriptorId operationId,
        ICalculationContext context,
        ICalculationInput input)
    {
        OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public CalculationOperationDescriptorId OperationId { get; }

    public ICalculationContext Context { get; }

    public ICalculationInput Input { get; }
}

public interface ICalculationExecutionMetadata
{
    CalculationOperationDescriptorId OperationId { get; }

    CalculationOperationVersion DescriptorVersion { get; }

    DateTime ExecutionTimestampUtc { get; }

    TimeSpan ExecutionDuration { get; }

    CalculationOperationCapabilityId CapabilityId { get; }

    CalculationOperationCapabilityCategory CapabilityCategory { get; }

    CalculationOperationCompatibilityStatus CompatibilityStatus { get; }
}

public sealed class CalculationExecutionMetadata : ICalculationExecutionMetadata
{
    public CalculationExecutionMetadata(
        CalculationOperationDescriptorId operationId,
        CalculationOperationVersion descriptorVersion,
        DateTime executionTimestampUtc,
        TimeSpan executionDuration,
        CalculationOperationCapabilityId capabilityId,
        CalculationOperationCapabilityCategory capabilityCategory,
        CalculationOperationCompatibilityStatus compatibilityStatus)
    {
        OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
        DescriptorVersion = descriptorVersion ?? throw new ArgumentNullException(nameof(descriptorVersion));
        CapabilityId = capabilityId ?? throw new ArgumentNullException(nameof(capabilityId));

        if (executionTimestampUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("ExecutionTimestampUtc must be UTC.");
        }

        if (executionDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(executionDuration));
        }

        ExecutionTimestampUtc = executionTimestampUtc;
        ExecutionDuration = executionDuration;
        CapabilityCategory = capabilityCategory;
        CompatibilityStatus = compatibilityStatus;
    }

    public CalculationOperationDescriptorId OperationId { get; }

    public CalculationOperationVersion DescriptorVersion { get; }

    public DateTime ExecutionTimestampUtc { get; }

    public TimeSpan ExecutionDuration { get; }

    public CalculationOperationCapabilityId CapabilityId { get; }

    public CalculationOperationCapabilityCategory CapabilityCategory { get; }

    public CalculationOperationCompatibilityStatus CompatibilityStatus { get; }
}

public interface ICalculationResult
{
    ICalculationOutput Output { get; }

    ICalculationExecutionMetadata ExecutionMetadata { get; }
}

public sealed class CalculationResult : ICalculationResult
{
    public CalculationResult(ICalculationOutput output, ICalculationExecutionMetadata executionMetadata)
    {
        Output = output ?? throw new ArgumentNullException(nameof(output));
        ExecutionMetadata = executionMetadata ?? throw new ArgumentNullException(nameof(executionMetadata));
    }

    public ICalculationOutput Output { get; }

    public ICalculationExecutionMetadata ExecutionMetadata { get; }
}

public interface ICalculationOperation
{
    ICalculationResult Execute(ICalculationRequest request);
}

public interface ICalculationPrimitive : ICalculationOperation
{
}

public interface ICalculationComposite : ICalculationOperation
{
}

public interface ICalculationEngine
{
    ICalculationResult Execute(ICalculationRequest request);
}

public interface ICalculationRuntime
{
    ICalculationResult Execute(CalculationRuntimeRequest request);
}

public readonly record struct CalculationCapabilityId
{
    private CalculationCapabilityId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CalculationCapabilityId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("CapabilityId is required.", nameof(value));
        }

        return new CalculationCapabilityId(value.Trim());
    }
}

public sealed class CalculationRuntimeRequest
{
    public CalculationRuntimeRequest(
        CalculationCapabilityId capabilityId,
        ICalculationContext context,
        ICalculationInput input)
    {
        if (string.IsNullOrWhiteSpace(capabilityId.Value))
        {
            throw new ArgumentException("CapabilityId is required.", nameof(capabilityId));
        }

        CapabilityId = capabilityId;
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }

    public CalculationCapabilityId CapabilityId { get; }

    public ICalculationContext Context { get; }

    public ICalculationInput Input { get; }
}

public interface ICalculationRegistry
{
    bool TryResolve(CalculationOperationDescriptorId operationId, out ICalculationOperation operation);

    bool TryResolve(CalculationOperationCapabilityId capabilityId, out ICalculationOperation operation);
}
