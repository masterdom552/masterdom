using Masterdom.Modules.Lease.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Infrastructure.Persistence.Lease;

/// <summary>
/// Adapts lease application operations to platform-level abstractions.
/// </summary>
public sealed class LeasePlatformOrchestrator : ILeasePlatformOrchestrator
{
    private const string ModuleId = "lease";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public LeasePlatformOrchestrator(
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

    public void OnLeaseMutated(LeaseAggregate lease, string operationName)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var leaseId = lease.Id.Value.ToString("N");

        TryResolveConfiguration(nowUtc, leaseId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, leaseId);
        TryExecuteWorkflow(nowUtc, leaseId);

        _domainEventPublisher.Publish(
            lease,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = lease.Id.ToString(),
                AggregateType = nameof(LeaseAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, string leaseId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("lease.commercial.enabled"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = leaseId,
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
                new MetadataKey("lease.aggregate.lease"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string leaseId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.lease.commercial"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = leaseId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string leaseId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.lease.lifecycle"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = leaseId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
