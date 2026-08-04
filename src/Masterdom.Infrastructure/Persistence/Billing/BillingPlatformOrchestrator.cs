using Masterdom.Modules.Billing.Application.Events;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Infrastructure.Persistence.Billing;

/// <summary>
/// Adapts billing application operations to platform-level abstractions.
/// </summary>
public sealed class BillingPlatformOrchestrator : IBillingPlatformOrchestrator
{
    private const string ModuleId = "billing";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public BillingPlatformOrchestrator(
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

    public void OnBillMutated(BillAggregate bill, string operationName)
    {
        ArgumentNullException.ThrowIfNull(bill);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var billId = bill.Id.Value.ToString("N");

        TryResolveConfiguration(nowUtc, billId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, billId);
        TryExecuteWorkflow(nowUtc, billId);

        _domainEventPublisher.Publish(
            bill,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = bill.Id.ToString(),
                AggregateType = nameof(BillAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    public void Publish(IBillingApplicationEvent applicationEvent)
    {
        ArgumentNullException.ThrowIfNull(applicationEvent);

        var nowUtc = DateTime.UtcNow;
        var scopePropertyId = applicationEvent.PropertyReference?.PropertyId.ToString("N")
            ?? applicationEvent.PersistedBillIds.First().Value.ToString("N");

        TryResolveConfiguration(nowUtc, scopePropertyId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, scopePropertyId);
        TryExecuteWorkflow(nowUtc, scopePropertyId);
    }

    private void TryResolveConfiguration(DateTime asOfUtc, string billId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("billing.adjustments.enabled"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = billId,
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
                new MetadataKey("billing.aggregate.bill"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string billId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.billing.lifecycle"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = billId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string billId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.billing.lifecycle"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = billId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
