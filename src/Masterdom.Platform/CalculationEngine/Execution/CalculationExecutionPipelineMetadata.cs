using System.Collections.Immutable;

namespace Masterdom.Platform.CalculationEngine.Execution;

internal static class CalculationExecutionPipelineCapabilityIdentifiers
{
    internal const string PipelineExecution = "execution.pipeline";
}

internal static class CalculationExecutionStageIdentifiers
{
    internal const string InputValidation = "validation.input";

    internal const string OperationResolution = "resolution.operation";

    internal const string OperationExecution = "execution.operation";

    internal const string OutputValidation = "validation.output";

    internal const string MetadataCapture = "metadata.capture";
}

internal static class CalculationExecutionPipelineMetadata
{
    internal static readonly CalculationExecutionPipelineDescriptor Descriptor = new(
        capabilityId: CalculationExecutionPipelineCapabilityIdentifiers.PipelineExecution,
        pipelineId: "calculation.execution.pipeline",
        pipelineVersion: "1.0",
        supportedContractVersion: "1.0",
        validationStages:
        [
            CalculationExecutionStageIdentifiers.InputValidation,
            CalculationExecutionStageIdentifiers.OutputValidation
        ],
        executionStages:
        [
            CalculationExecutionStageIdentifiers.OperationResolution,
            CalculationExecutionStageIdentifiers.OperationExecution
        ],
        metadataStages:
        [
            CalculationExecutionStageIdentifiers.MetadataCapture
        ],
        descriptorVersion: "1.0");
}

internal sealed class CalculationExecutionPipelineDescriptor
{
    private readonly ImmutableArray<string> _validationStages;
    private readonly ImmutableArray<string> _executionStages;
    private readonly ImmutableArray<string> _metadataStages;

    public CalculationExecutionPipelineDescriptor(
        string capabilityId,
        string pipelineId,
        string pipelineVersion,
        string supportedContractVersion,
        IReadOnlyList<string> validationStages,
        IReadOnlyList<string> executionStages,
        IReadOnlyList<string> metadataStages,
        string descriptorVersion)
    {
        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new ArgumentException("CapabilityId cannot be empty.", nameof(capabilityId));
        }

        if (string.IsNullOrWhiteSpace(pipelineId))
        {
            throw new ArgumentException("PipelineId cannot be empty.", nameof(pipelineId));
        }

        if (string.IsNullOrWhiteSpace(pipelineVersion))
        {
            throw new ArgumentException("PipelineVersion cannot be empty.", nameof(pipelineVersion));
        }

        if (string.IsNullOrWhiteSpace(supportedContractVersion))
        {
            throw new ArgumentException("SupportedContractVersion cannot be empty.", nameof(supportedContractVersion));
        }

        if (string.IsNullOrWhiteSpace(descriptorVersion))
        {
            throw new ArgumentException("DescriptorVersion cannot be empty.", nameof(descriptorVersion));
        }

        CapabilityId = capabilityId.Trim();
        PipelineId = pipelineId.Trim();
        PipelineVersion = pipelineVersion.Trim();
        SupportedContractVersion = supportedContractVersion.Trim();
        DescriptorVersion = descriptorVersion.Trim();
        _validationStages = (validationStages ?? throw new ArgumentNullException(nameof(validationStages))).ToImmutableArray();
        _executionStages = (executionStages ?? throw new ArgumentNullException(nameof(executionStages))).ToImmutableArray();
        _metadataStages = (metadataStages ?? throw new ArgumentNullException(nameof(metadataStages))).ToImmutableArray();
    }

    public string CapabilityId { get; }

    public string PipelineId { get; }

    public string PipelineVersion { get; }

    public string SupportedContractVersion { get; }

    public IReadOnlyList<string> ValidationStages => _validationStages;

    public IReadOnlyList<string> ExecutionStages => _executionStages;

    public IReadOnlyList<string> MetadataStages => _metadataStages;

    public string DescriptorVersion { get; }
}

internal enum CalculationExecutionRecordStatus
{
    Unknown = 0,
    Succeeded = 1,
    Failed = 2
}

internal sealed class CalculationExecutionRecord
{
    private readonly ImmutableArray<string> _executedStages;

    public CalculationExecutionRecord(
        Guid executionId,
        string capabilityId,
        string pipelineId,
        string pipelineVersion,
        string contractVersion,
        string metadataVersion,
        DateTime startedAt,
        DateTime completedAt,
        TimeSpan duration,
        IReadOnlyList<string> executedStages,
        CalculationExecutionRecordStatus executionStatus,
        string? failureReason = null)
    {
        if (executionId == Guid.Empty)
        {
            throw new ArgumentException("ExecutionId cannot be empty.", nameof(executionId));
        }

        if (string.IsNullOrWhiteSpace(capabilityId))
        {
            throw new ArgumentException("CapabilityId cannot be empty.", nameof(capabilityId));
        }

        if (string.IsNullOrWhiteSpace(pipelineId))
        {
            throw new ArgumentException("PipelineId cannot be empty.", nameof(pipelineId));
        }

        if (string.IsNullOrWhiteSpace(pipelineVersion))
        {
            throw new ArgumentException("PipelineVersion cannot be empty.", nameof(pipelineVersion));
        }

        if (string.IsNullOrWhiteSpace(contractVersion))
        {
            throw new ArgumentException("ContractVersion cannot be empty.", nameof(contractVersion));
        }

        if (string.IsNullOrWhiteSpace(metadataVersion))
        {
            throw new ArgumentException("MetadataVersion cannot be empty.", nameof(metadataVersion));
        }

        if (startedAt.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("StartedAt must be UTC.");
        }

        if (completedAt.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("CompletedAt must be UTC.");
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        ExecutionId = executionId;
        CapabilityId = capabilityId.Trim();
        PipelineId = pipelineId.Trim();
        PipelineVersion = pipelineVersion.Trim();
        ContractVersion = contractVersion.Trim();
        MetadataVersion = metadataVersion.Trim();
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Duration = duration;
        _executedStages = (executedStages ?? throw new ArgumentNullException(nameof(executedStages))).ToImmutableArray();
        ExecutionStatus = executionStatus;
        FailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();
    }

    public Guid ExecutionId { get; }

    public string CapabilityId { get; }

    public string PipelineId { get; }

    public string PipelineVersion { get; }

    public string ContractVersion { get; }

    public string MetadataVersion { get; }

    public DateTime StartedAt { get; }

    public DateTime CompletedAt { get; }

    public TimeSpan Duration { get; }

    public IReadOnlyList<string> ExecutedStages => _executedStages;

    public CalculationExecutionRecordStatus ExecutionStatus { get; }

    public string? FailureReason { get; }
}
