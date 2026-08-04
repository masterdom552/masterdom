using Masterdom.Modules.Properties.Domain.Entities.Property;
using Masterdom.Modules.Properties.Domain.Entities.Property.Events;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Events;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;

namespace Masterdom.Platform.Tests.Integration;

public sealed class PropertyPlatformConsumptionTests
{
    [Fact]
    public void PropertyDomain_ShouldConsumeConfigurationMetadataRulesWorkflowAndEvents()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 7, 27), DateTimeKind.Utc);

        var property = Property.Create(
            new PropertyCode("PLT-01"),
            new PropertyName("Platform Consumption Building"),
            PropertyType.MixedUse);

        property.CreateUnit(new UnitCode("U-01"), "U-01", UnitType.Office);

        var configurationResolver = new ConfigurationResolver(
            new InMemoryConfigurationRepository(new[]
            {
                new ConfigurationRecord(
                    new ConfigurationId(Guid.NewGuid()),
                    new ConfigurationKey("properties.archive.enabled"),
                    ConfigurationScope.Property(property.Id.Value.ToString("N")),
                    new ConfigurationVersion(1),
                    new ConfigurationValue("true"),
                    new EffectivePeriod(asOfUtc.AddDays(-1), null),
                    "tester",
                    "seed",
                    asOfUtc)
            }));

        var metadataResolver = new MetadataResolver(
            new InMemoryMetadataRepository(new[]
            {
                new MetadataDefinition(
                    new MetadataId(Guid.NewGuid()),
                    new MetadataKey("properties.aggregate.property"),
                    MetadataCategory.Aggregate,
                    MetadataScope.Module("properties"),
                    new MetadataVersion(1),
                    new MetadataEffectivePeriod(asOfUtc.AddDays(-1), null),
                    "property",
                    "Property aggregate metadata",
                    null,
                    false,
                    null,
                    null,
                    "tester",
                    asOfUtc)
            }));

        var ruleScope = RuleScope.Create(RuleScopeKind.Module, "properties");
        var ruleSet = new RuleSetDefinition(
            new RuleSetId(Guid.NewGuid()),
            new RuleSetKey("rules.properties.archive"),
            "Property Archive Rules",
            "Uses configuration to decide archive policy.",
            RuleCategory.Validation,
            ruleScope,
            new RuleVersion(1),
            new RuleEffectivePeriod(asOfUtc.AddDays(-1), null),
            false,
            null,
            null,
            "tester",
            asOfUtc);

        var rule = new RuleDefinition(
            new RuleId(Guid.NewGuid()),
            ruleSet.Id,
            new RuleKey("rule.properties.archive.enabled"),
            "Archive enabled",
            "Configuration-driven archive guard.",
            RuleKind.Boolean,
            RuleCondition.Boolean(
                new RuleInputKey("config:properties.archive.enabled"),
                true),
            RuleCategory.Validation,
            RuleSeverity.Error,
            new RulePriority(1),
            ruleScope,
            new RuleVersion(1),
            new RuleEffectivePeriod(asOfUtc.AddDays(-1), null),
            null,
            false,
            null,
            null,
            "tester",
            asOfUtc);

        var ruleResolver = new RuleResolver(
            new InMemoryRuleRepository(new[] { ruleSet }, new[] { rule }),
            configurationResolver,
            metadataResolver);

        var workflowId = new WorkflowId(Guid.NewGuid());
        var versionId = new WorkflowVersionId(Guid.NewGuid());
        var startStep = new WorkflowStepId(Guid.NewGuid());
        var endStep = new WorkflowStepId(Guid.NewGuid());

        var workflowResolver = new WorkflowResolver(
            new InMemoryWorkflowRepository(
                new[]
                {
                    new WorkflowDefinition(
                        workflowId,
                        new WorkflowKey("workflow.properties.lifecycle"),
                        "Property Lifecycle",
                        "Property lifecycle orchestration baseline.",
                        WorkflowScope.Create(WorkflowScopeKind.Module, "properties"),
                        "tester",
                        asOfUtc)
                },
                new[]
                {
                    new WorkflowVersionDefinition(
                        versionId,
                        workflowId,
                        new WorkflowVersion(1),
                        new WorkflowEffectivePeriod(asOfUtc.AddDays(-1), null),
                        false,
                        null,
                        null,
                        "tester",
                        asOfUtc)
                },
                new[]
                {
                    new WorkflowStepDefinition(
                        startStep,
                        versionId,
                        "start",
                        "Start",
                        WorkflowStepKind.Automatic,
                        true,
                        false,
                        WorkflowRetryPolicy.None(),
                        WorkflowTimeoutPolicy.None(),
                        null),
                    new WorkflowStepDefinition(
                        endStep,
                        versionId,
                        "end",
                        "End",
                        WorkflowStepKind.Automatic,
                        false,
                        true,
                        WorkflowRetryPolicy.None(),
                        WorkflowTimeoutPolicy.None(),
                        null)
                },
                new[]
                {
                    new WorkflowTransitionDefinition(
                        new WorkflowTransitionId(Guid.NewGuid()),
                        versionId,
                        startStep,
                        endStep,
                        WorkflowBranchKind.Sequential,
                        new WorkflowPriority(1),
                        WorkflowTransitionConditionKind.Always,
                        null,
                        null)
                }),
            configurationResolver,
            metadataResolver,
            ruleResolver,
            new InMemoryWorkflowStateStore());

        var eventRegistry = new EventRegistry();
        eventRegistry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType(nameof(PropertyCreatedDomainEvent)),
            Category = EventCategory.Domain,
            Version = new EventVersion(1)
        });
        eventRegistry.RegisterEvent(new EventDescriptor
        {
            EventType = new EventType(nameof(UnitCreatedDomainEvent)),
            Category = EventCategory.Domain,
            Version = new EventVersion(1)
        });

        var eventPublisher = new EventPublisher(
            new EventStore(new InMemoryEventRepository()),
            new EventDispatcher(new EventHandlerResolver(eventRegistry)));

        var domainEventPublisher = new DomainEventPublisher(
            new DomainEventAdapter(),
            eventPublisher);

        var configuration = configurationResolver.Resolve(
            new ConfigurationKey("properties.archive.enabled"),
            new ConfigurationResolutionRequest
            {
                ModuleId = "properties",
                PropertyId = property.Id.Value.ToString("N"),
                AsOfUtc = asOfUtc
            });

        var metadata = metadataResolver.Resolve(
            new MetadataKey("properties.aggregate.property"),
            MetadataScope.Module("properties"),
            asOfUtc);

        var ruleResult = ruleResolver.Evaluate(
            ruleSet.Key,
            ruleScope,
            new RuleContext
            {
                ModuleId = "properties",
                PropertyId = property.Id.Value.ToString("N"),
                AsOfUtc = asOfUtc
            },
            new RuleInput(Array.Empty<RuleInputItem>()));

        var workflowResult = workflowResolver.Execute(
            new WorkflowKey("workflow.properties.lifecycle"),
            WorkflowScope.Create(WorkflowScopeKind.Module, "properties"),
            new WorkflowContext
            {
                ModuleId = "properties",
                PropertyId = property.Id.Value.ToString("N"),
                AsOfUtc = asOfUtc
            });

        var eventResult = domainEventPublisher.Publish(
            property,
            new EventContext
            {
                ModuleId = "properties",
                AggregateId = property.Id.ToString(),
                AggregateType = nameof(Property),
                CorrelationId = "corr-pdp009",
                CausationId = "cause-pdp009",
                OccurredAtUtc = asOfUtc
            });

        Assert.Equal("true", configuration.Record.Value.Value);
        Assert.Equal("property", metadata.Name);
        Assert.True(ruleResult.Passed);
        Assert.Equal(WorkflowExecutionStatus.Completed, workflowResult.State.Status);
        Assert.Equal(2, eventResult.PublishedCount);
    }
}
