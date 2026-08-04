using Masterdom.Modules.Tenancy.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Infrastructure.Persistence.Tenancy;

/// <summary>
/// Adapts tenancy application operations to platform-level abstractions.
/// </summary>
public sealed class TenancyPlatformOrchestrator : ITenancyPlatformOrchestrator
{
    private const string ModuleId = "tenancy";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public TenancyPlatformOrchestrator(
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

    public void OnTenancyMutated(TenancyAggregate tenancy, string operationName)
    {
        ArgumentNullException.ThrowIfNull(tenancy);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;
        var tenancyId = tenancy.Id.Value.ToString("N");

        TryResolveConfiguration(nowUtc, tenancyId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, tenancyId);
        TryExecuteWorkflow(nowUtc, tenancyId);

        _domainEventPublisher.Publish(
            tenancy,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = tenancy.Id.ToString(),
                AggregateType = nameof(TenancyAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, string tenancyId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("tenancy.lifecycle.enabled"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
                    PropertyId = tenancyId,
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
                new MetadataKey("tenancy.aggregate.tenancy"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc, string tenancyId)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.tenancy.lifecycle"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    PropertyId = tenancyId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc, string tenancyId)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.tenancy.lifecycle"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    PropertyId = tenancyId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
