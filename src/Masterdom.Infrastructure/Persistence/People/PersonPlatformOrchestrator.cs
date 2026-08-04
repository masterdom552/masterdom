using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;

namespace Masterdom.Infrastructure.Persistence.People;

/// <summary>
/// Adapts people application operations to platform abstractions.
/// </summary>
public sealed class PersonPlatformOrchestrator : IPersonPlatformOrchestrator
{
    private const string ModuleId = "people";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public PersonPlatformOrchestrator(
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

    public void OnPersonMutated(Person person, string operationName)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var personId = person.Id.Value.ToString("N");

        TryResolveConfiguration(nowUtc, personId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, personId);
        TryExecuteWorkflow(nowUtc, personId);

        _domainEventPublisher.Publish(
            person,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = person.Id.ToString(),
                AggregateType = nameof(Person),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, string personId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("people.identity.enabled"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = personId,
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
                new MetadataKey("people.aggregate.person"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string personId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.people.identity"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = personId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string personId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.people.lifecycle"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = personId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
