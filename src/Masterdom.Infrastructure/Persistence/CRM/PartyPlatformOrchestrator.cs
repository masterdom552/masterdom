using Masterdom.Modules.CRM.Application.Support;
using Masterdom.Modules.CRM.Domain.Entities.Party;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;

namespace Masterdom.Infrastructure.Persistence.CRM;

/// <summary>
/// Adapts CRM party application operations to platform abstractions.
/// </summary>
public sealed class PartyPlatformOrchestrator : IPartyPlatformOrchestrator
{
    private const string ModuleId = "crm";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public PartyPlatformOrchestrator(
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

    public void OnPartyMutated(Party party, string operationName)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var partyId = party.Id.Value.ToString("N");

        TryResolveConfiguration(nowUtc, partyId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, partyId);
        TryExecuteWorkflow(nowUtc, partyId);

        _domainEventPublisher.Publish(
            party,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = party.Id.ToString(),
                AggregateType = nameof(Party),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, string partyId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("crm.party.enabled"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = partyId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
        }
    }

    private void TryResolveMetadata(DateTime asOfUtc)
    {
        try
        {
            _metadataResolver.Resolve(
                new MetadataKey("crm.aggregate.party"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string partyId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.crm.party"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = partyId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string partyId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.crm.party-lifecycle"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = partyId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
        }
    }
}
