using System;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;
using Masterdom.Platform.Workflow;

namespace Masterdom.Platform.Tests.Workflow;

public sealed class WorkflowResolverTests
{
    [Fact]
    public void Execute_WithAlwaysTransition_ShouldCompleteWorkflow()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);
        var workflowId = new WorkflowId(Guid.NewGuid());
        var versionId = new WorkflowVersionId(Guid.NewGuid());
        var startStep = new WorkflowStepId(Guid.NewGuid());
        var endStep = new WorkflowStepId(Guid.NewGuid());

        var repository = new InMemoryWorkflowRepository(
            new[]
            {
                new WorkflowDefinition(
                    workflowId,
                    new WorkflowKey("workflow.people.default"),
                    "People Workflow",
                    null,
                    WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
                    "tester",
                    asOfUtc)
            },
            new[]
            {
                new WorkflowVersionDefinition(
                    versionId,
                    workflowId,
                    new WorkflowVersion(1),
                    new WorkflowEffectivePeriod(asOfUtc, null),
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
            });

        var resolver = new WorkflowResolver(
            repository,
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()),
            new RuleResolver(
                new InMemoryRuleRepository(),
                new ConfigurationResolver(new InMemoryConfigurationRepository()),
                new MetadataResolver(new InMemoryMetadataRepository())),
            new InMemoryWorkflowStateStore());

        var result = resolver.Execute(
            new WorkflowKey("workflow.people.default"),
            WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
            new WorkflowContext
            {
                ModuleId = "people",
                AsOfUtc = asOfUtc
            });

        Assert.Equal(WorkflowExecutionStatus.Completed, result.State.Status);
        Assert.Equal(2, result.State.CompletedSteps.Count);
        Assert.True(result.IsTerminal);
    }

    [Fact]
    public void Execute_WithManualApprovalStep_ShouldPauseWithPendingStep()
    {
        var asOfUtc = DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc);
        var workflowId = new WorkflowId(Guid.NewGuid());
        var versionId = new WorkflowVersionId(Guid.NewGuid());
        var startStep = new WorkflowStepId(Guid.NewGuid());
        var approvalStep = new WorkflowStepId(Guid.NewGuid());

        var repository = new InMemoryWorkflowRepository(
            new[]
            {
                new WorkflowDefinition(
                    workflowId,
                    new WorkflowKey("workflow.people.approval"),
                    "People Approval Workflow",
                    null,
                    WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
                    "tester",
                    asOfUtc)
            },
            new[]
            {
                new WorkflowVersionDefinition(
                    versionId,
                    workflowId,
                    new WorkflowVersion(1),
                    new WorkflowEffectivePeriod(asOfUtc, null),
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
                    approvalStep,
                    versionId,
                    "approval",
                    "Approval",
                    WorkflowStepKind.ManualApproval,
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
                    approvalStep,
                    WorkflowBranchKind.Sequential,
                    new WorkflowPriority(1),
                    WorkflowTransitionConditionKind.Always,
                    null,
                    null)
            });

        var resolver = new WorkflowResolver(
            repository,
            new ConfigurationResolver(new InMemoryConfigurationRepository()),
            new MetadataResolver(new InMemoryMetadataRepository()),
            new RuleResolver(
                new InMemoryRuleRepository(),
                new ConfigurationResolver(new InMemoryConfigurationRepository()),
                new MetadataResolver(new InMemoryMetadataRepository())),
            new InMemoryWorkflowStateStore());

        var result = resolver.Execute(
            new WorkflowKey("workflow.people.approval"),
            WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
            new WorkflowContext
            {
                ModuleId = "people",
                AsOfUtc = asOfUtc
            });

        Assert.Equal(WorkflowExecutionStatus.Running, result.State.Status);
        Assert.Single(result.State.PendingSteps);
        Assert.Equal(approvalStep.Value, result.State.PendingSteps[0].Value);
        Assert.False(result.IsTerminal);
    }
}
