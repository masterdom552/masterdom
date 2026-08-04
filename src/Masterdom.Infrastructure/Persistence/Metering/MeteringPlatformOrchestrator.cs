using Masterdom.Modules.Metering.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Infrastructure.Persistence.Metering;

public sealed class MeteringPlatformOrchestrator : IMeteringPlatformOrchestrator
{
    private const string ModuleId = "metering";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public MeteringPlatformOrchestrator(
        IConfigurationResolver configurationResolver,
        IMetadataResolver metadataResolver,
        IRuleResolver ruleResolver,
        IWorkflowResolver workflowResolver,
        IDomainEventPublisher domainEventPublisher)
    {
        _configurationResolver = configurationResolver ?? throw new ArgumentNullException(nameof(configurationResolver));
        _metadataResolver = metadataResolver ?? throw new ArgumentNullException(nameof(metadataResolver));
        _ruleResolver = ruleResolver ?? throw new ArgumentNullException(nameof(ruleResolver));
        _workflowResolver = workflowResolver ?? throw new ArgumentNullException(nameof(workflowResolver));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
    }

    public void OnMeterMutated(MeterAggregate meter, string operationName)
    {
        ArgumentNullException.ThrowIfNull(meter);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var meterId = meter.Id.Value.ToString("N");

        TryResolveConfiguration(nowUtc, meterId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, meterId);
        TryExecuteWorkflow(nowUtc, meterId);

        _domainEventPublisher.Publish(
            meter,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = meter.Id.ToString(),
                AggregateType = nameof(MeterAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, string meterId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("metering.readings.allow_future"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = meterId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryResolveMetadata(DateTime asOfUtc)
    {
        try
        {
            _metadataResolver.Resolve(
                new MetadataKey("metering.aggregate.meter"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string meterId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.metering.readings"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = meterId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string meterId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.metering.readings"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = meterId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
