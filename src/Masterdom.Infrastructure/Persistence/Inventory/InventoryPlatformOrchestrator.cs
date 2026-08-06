using Masterdom.Modules.Inventory.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Infrastructure.Persistence.Inventory;

public sealed class InventoryPlatformOrchestrator : IInventoryPlatformOrchestrator
{
    private const string ModuleId = "inventory";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public InventoryPlatformOrchestrator(
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

    public void OnInventoryItemMutated(InventoryItemAggregate inventoryItem, string operationName)
    {
        ArgumentNullException.ThrowIfNull(inventoryItem);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;

        TryResolveConfiguration(nowUtc, inventoryItem.PropertyId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, inventoryItem.PropertyId);
        TryExecuteWorkflow(nowUtc, inventoryItem.PropertyId);

        _domainEventPublisher.Publish(
            inventoryItem,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = inventoryItem.Id.ToString(),
                AggregateType = nameof(InventoryItemAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, Guid propertyId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("inventory.items.default"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = propertyId.ToString("N"),
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
                new MetadataKey("inventory.aggregate.item"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, Guid propertyId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.inventory.default"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = propertyId.ToString("N"),
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, Guid propertyId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.inventory.default"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = propertyId.ToString("N"),
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
