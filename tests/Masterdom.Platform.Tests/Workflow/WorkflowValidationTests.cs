using System;
using Masterdom.Platform.Workflow;

namespace Masterdom.Platform.Tests.Workflow;

public sealed class WorkflowValidationTests
{
    [Fact]
    public void ValidateAll_WhenVersionHasNoStartStep_ShouldThrow()
    {
        var workflowId = new WorkflowId(Guid.NewGuid());
        var versionId = new WorkflowVersionId(Guid.NewGuid());
        var endStepId = new WorkflowStepId(Guid.NewGuid());

        var workflow = new WorkflowDefinition(
            workflowId,
            new WorkflowKey("workflow.people.default"),
            "People Workflow",
            null,
            WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
            "tester",
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));

        var version = new WorkflowVersionDefinition(
            versionId,
            workflowId,
            new WorkflowVersion(1),
            new WorkflowEffectivePeriod(DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc), null),
            false,
            null,
            null,
            "tester",
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));

        var endStep = new WorkflowStepDefinition(
            endStepId,
            versionId,
            "end",
            "End",
            WorkflowStepKind.Automatic,
            false,
            true,
            WorkflowRetryPolicy.None(),
            WorkflowTimeoutPolicy.None(),
            null);

        var exception = Assert.Throws<WorkflowValidationException>(() =>
            WorkflowValidation.ValidateAll(
                new[] { workflow },
                new[] { version },
                new[] { endStep },
                Array.Empty<WorkflowTransitionDefinition>()));

        Assert.Contains("exactly one start step", exception.Message);
    }

    [Fact]
    public void ValidateAll_WhenTransitionsContainCycle_ShouldThrow()
    {
        var workflowId = new WorkflowId(Guid.NewGuid());
        var versionId = new WorkflowVersionId(Guid.NewGuid());
        var startStepId = new WorkflowStepId(Guid.NewGuid());
        var middleStepId = new WorkflowStepId(Guid.NewGuid());
        var endStepId = new WorkflowStepId(Guid.NewGuid());

        var workflow = new WorkflowDefinition(
            workflowId,
            new WorkflowKey("workflow.people.default"),
            "People Workflow",
            null,
            WorkflowScope.Create(WorkflowScopeKind.Module, "people"),
            "tester",
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));

        var version = new WorkflowVersionDefinition(
            versionId,
            workflowId,
            new WorkflowVersion(1),
            new WorkflowEffectivePeriod(DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc), null),
            false,
            null,
            null,
            "tester",
            DateTime.SpecifyKind(new DateTime(2026, 1, 1), DateTimeKind.Utc));

        var start = new WorkflowStepDefinition(
            startStepId,
            versionId,
            "start",
            "Start",
            WorkflowStepKind.Automatic,
            true,
            false,
            WorkflowRetryPolicy.None(),
            WorkflowTimeoutPolicy.None(),
            null);

        var middle = new WorkflowStepDefinition(
            middleStepId,
            versionId,
            "middle",
            "Middle",
            WorkflowStepKind.Automatic,
            false,
            false,
            WorkflowRetryPolicy.None(),
            WorkflowTimeoutPolicy.None(),
            null);

        var end = new WorkflowStepDefinition(
            endStepId,
            versionId,
            "end",
            "End",
            WorkflowStepKind.Automatic,
            false,
            true,
            WorkflowRetryPolicy.None(),
            WorkflowTimeoutPolicy.None(),
            null);

        var transitions = new[]
        {
            new WorkflowTransitionDefinition(
                new WorkflowTransitionId(Guid.NewGuid()),
                versionId,
                startStepId,
                middleStepId,
                WorkflowBranchKind.Sequential,
                new WorkflowPriority(1),
                WorkflowTransitionConditionKind.Always,
                null,
                null),
            new WorkflowTransitionDefinition(
                new WorkflowTransitionId(Guid.NewGuid()),
                versionId,
                middleStepId,
                startStepId,
                WorkflowBranchKind.Sequential,
                new WorkflowPriority(1),
                WorkflowTransitionConditionKind.Always,
                null,
                null),
            new WorkflowTransitionDefinition(
                new WorkflowTransitionId(Guid.NewGuid()),
                versionId,
                middleStepId,
                endStepId,
                WorkflowBranchKind.Sequential,
                new WorkflowPriority(2),
                WorkflowTransitionConditionKind.Always,
                null,
                null)
        };

        var exception = Assert.Throws<WorkflowValidationException>(() =>
            WorkflowValidation.ValidateAll(
                new[] { workflow },
                new[] { version },
                new[] { start, middle, end },
                transitions));

        Assert.Contains("Circular workflow transitions", exception.Message);
    }
}
