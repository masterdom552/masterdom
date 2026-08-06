using Masterdom.Modules.Maintenance.Application.Support;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Infrastructure.Persistence.Maintenance;

public sealed class MaintenancePlatformOrchestrator : IMaintenancePlatformOrchestrator
{
    private const string ModuleId = "maintenance";

    private readonly IConfigurationResolver _configurationResolver;
    private readonly IMetadataResolver _metadataResolver;
    private readonly IRuleResolver _ruleResolver;
    private readonly IWorkflowResolver _workflowResolver;
    private readonly IDomainEventPublisher _domainEventPublisher;

    public MaintenancePlatformOrchestrator(
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

    public void OnMaintenanceTicketMutated(MaintenanceTicketAggregate maintenanceTicket, string operationName)
    {
        ArgumentNullException.ThrowIfNull(maintenanceTicket);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var nowUtc = DateTime.UtcNow;

        TryResolveConfiguration(nowUtc, maintenanceTicket.PropertyId);
        TryResolveMetadata(nowUtc);
        TryEvaluateRules(nowUtc, maintenanceTicket.PropertyId);
        TryExecuteWorkflow(nowUtc, maintenanceTicket.PropertyId);

        _domainEventPublisher.Publish(
            maintenanceTicket,
            new EventContext
            {
                ModuleId = ModuleId,
                AggregateId = maintenanceTicket.Id.ToString(),
                AggregateType = nameof(MaintenanceTicketAggregate),
                CorrelationId = operationName,
                OccurredAtUtc = nowUtc
            });
    }

    private void TryResolveConfiguration(DateTime asOfUtc, Guid propertyId)
    {
        try
        {
            _configurationResolver.Resolve(
                new ConfigurationKey("maintenance.ticket.default"),
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
                new MetadataKey("maintenance.aggregate.ticket"),
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
                new RuleSetKey("rules.maintenance.default"),
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
                new WorkflowKey("workflow.maintenance.default"),
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
