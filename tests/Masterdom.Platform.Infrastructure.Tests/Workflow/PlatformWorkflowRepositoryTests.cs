using System;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Workflow;
using Masterdom.Platform.Workflow;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Tests.Workflow;

public sealed class PlatformWorkflowRepositoryTests
{
    [Fact]
    public void GetAll_ShouldMapPersistedEntitiesToWorkflowDefinitions()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MasterdomDbContext(options);

        var changedAt = DateTime.SpecifyKind(new DateTime(2026, 2, 1), DateTimeKind.Utc);
        var workflowId = Guid.NewGuid();
        var versionId = Guid.NewGuid();
        var startStepId = Guid.NewGuid();
        var endStepId = Guid.NewGuid();

        dbContext.PlatformWorkflows.Add(new PlatformWorkflowEntity
        {
            Id = workflowId,
            Key = "workflow.people.default",
            Name = "People Workflow",
            Description = "Default people workflow",
            ScopeKind = (int)WorkflowScopeKind.Module,
            ScopeIdentifier = "people",
            ChangedBy = "tester",
            ChangedAtUtc = changedAt
        });

        dbContext.PlatformWorkflowVersions.Add(new PlatformWorkflowVersionEntity
        {
            Id = versionId,
            WorkflowId = workflowId,
            Version = 1,
            EffectiveFromUtc = changedAt,
            EffectiveToUtc = null,
            IsDeprecated = false,
            ReplacedByVersionId = null,
            Compatibility = "v1",
            ChangedBy = "tester",
            ChangedAtUtc = changedAt
        });

        dbContext.PlatformWorkflowSteps.Add(new PlatformWorkflowStepEntity
        {
            Id = startStepId,
            WorkflowVersionId = versionId,
            Key = "start",
            Name = "Start",
            Kind = (int)WorkflowStepKind.Automatic,
            IsStart = true,
            IsTerminal = false,
            RetryMaxAttempts = 0,
            RetryDelayMilliseconds = 0,
            TimeoutMilliseconds = 0
        });

        dbContext.PlatformWorkflowSteps.Add(new PlatformWorkflowStepEntity
        {
            Id = endStepId,
            WorkflowVersionId = versionId,
            Key = "end",
            Name = "End",
            Kind = (int)WorkflowStepKind.Automatic,
            IsStart = false,
            IsTerminal = true,
            RetryMaxAttempts = 0,
            RetryDelayMilliseconds = 0,
            TimeoutMilliseconds = 0
        });

        dbContext.PlatformWorkflowTransitions.Add(new PlatformWorkflowTransitionEntity
        {
            Id = Guid.NewGuid(),
            WorkflowVersionId = versionId,
            FromStepId = startStepId,
            ToStepId = endStepId,
            BranchKind = (int)WorkflowBranchKind.Sequential,
            Priority = 1,
            ConditionKind = (int)WorkflowTransitionConditionKind.Always,
            RuleSetKey = null,
            RuleScopeKind = null,
            RuleScopeIdentifier = null
        });

        dbContext.SaveChanges();

        var repository = new PlatformWorkflowRepository(dbContext);

        var workflows = repository.GetAllWorkflows();
        var versions = repository.GetAllVersions();
        var steps = repository.GetAllSteps();
        var transitions = repository.GetAllTransitions();

        var workflow = Assert.Single(workflows);
        Assert.Equal("workflow.people.default", workflow.Key.Value);
        Assert.Equal(WorkflowScopeKind.Module, workflow.Scope.Kind);

        var version = Assert.Single(versions);
        Assert.Equal(workflowId, version.WorkflowId.Value);
        Assert.Equal(1, version.Version.Value);

        Assert.Equal(2, steps.Count);
        Assert.Single(transitions);
    }
}
