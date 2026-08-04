using Masterdom.Modules.SubsidyOptimization.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using OptimizationRunAggregate = Masterdom.Modules.SubsidyOptimization.Domain.Entities.SubsidyOptimization.OptimizationRun;

namespace Masterdom.Infrastructure.Persistence.SubsidyOptimization;

public sealed class SubsidyOptimizationPlatformOrchestrator : ISubsidyOptimizationPlatformOrchestrator
{
    private const string ModuleId = "subsidyoptimization";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public SubsidyOptimizationPlatformOrchestrator(
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

    public void OnOptimizationRunMutated(OptimizationRunAggregate optimizationRun, string operationName)
    {
        ArgumentNullException.ThrowIfNull(optimizationRun);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;

        TryResolveConfiguration(nowUtc);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc);
        TryExecuteWorkflow(nowUtc);

        _domainEventPublisher.Publish(
            optimizationRun,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = optimizationRun.Id.ToString(),
                AggregateType = nameof(OptimizationRunAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("subsidyoptimization.policy.default"),
                new ConfigurationResolutionRequest
                {
                    ModuleId = ModuleId,
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
                new MetadataKey("subsidyoptimization.aggregate.optimizationrun"),
                MetadataScope.Module(ModuleId),
                asOfUtc);
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryEvaluateRules(DateTime asOfUtc)
    {
        try
        {
            _ruleResolver.Evaluate(
                new RuleSetKey("rules.subsidyoptimization.default"),
                RuleScope.Create(RuleScopeKind.Module, ModuleId),
                new RuleContext
                {
                    ModuleId = ModuleId,
                    AsOfUtc = asOfUtc
                },
                new RuleInput(Array.Empty<RuleInputItem>()));
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }

    private void TryExecuteWorkflow(DateTime asOfUtc)
    {
        try
        {
            _workflowResolver.Execute(
                new WorkflowKey("workflow.subsidyoptimization.default"),
                WorkflowScope.Create(WorkflowScopeKind.Module, ModuleId),
                new WorkflowContext
                {
                    ModuleId = ModuleId,
                    AsOfUtc = asOfUtc
                });
        }
        catch
        {
            // Best-effort platform integration must not alter aggregate behavior.
        }
    }
}
