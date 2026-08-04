using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Workflow;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Workflow;

/// <summary>
/// EF Core-backed workflow repository implementation.
/// </summary>
public sealed class PlatformWorkflowRepository : IWorkflowRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PlatformWorkflowRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<WorkflowDefinition> GetAllWorkflows()
    {
        var entities = _dbContext
            .Set<PlatformWorkflowEntity>()
            .AsNoTracking()
            .OrderBy(x => x.Key)
            .ThenBy(x => x.ScopeKind)
            .ThenBy(x => x.ScopeIdentifier)
            .ToList();

        return entities
            .Select(entity => new WorkflowDefinition(
                new WorkflowId(entity.Id),
                new WorkflowKey(entity.Key),
                entity.Name,
                entity.Description,
                WorkflowScope.Create((WorkflowScopeKind)entity.ScopeKind, entity.ScopeIdentifier),
                entity.ChangedBy,
                entity.ChangedAtUtc))
            .ToList();
    }

    public IReadOnlyList<WorkflowVersionDefinition> GetAllVersions()
    {
        var entities = _dbContext
            .Set<PlatformWorkflowVersionEntity>()
            .AsNoTracking()
            .OrderBy(x => x.WorkflowId)
            .ThenByDescending(x => x.EffectiveFromUtc)
            .ThenByDescending(x => x.Version)
            .ToList();

        return entities
            .Select(entity => new WorkflowVersionDefinition(
                new WorkflowVersionId(entity.Id),
                new WorkflowId(entity.WorkflowId),
                new WorkflowVersion(entity.Version),
                new WorkflowEffectivePeriod(entity.EffectiveFromUtc, entity.EffectiveToUtc),
                entity.IsDeprecated,
                entity.ReplacedByVersionId.HasValue
                    ? new WorkflowVersionId(entity.ReplacedByVersionId.Value)
                    : null,
                entity.Compatibility,
                entity.ChangedBy,
                entity.ChangedAtUtc))
            .ToList();
    }

    public IReadOnlyList<WorkflowStepDefinition> GetAllSteps()
    {
        var entities = _dbContext
            .Set<PlatformWorkflowStepEntity>()
            .AsNoTracking()
            .OrderBy(x => x.WorkflowVersionId)
            .ThenBy(x => x.IsStart ? 0 : 1)
            .ThenBy(x => x.Key)
            .ToList();

        return entities
            .Select(entity => new WorkflowStepDefinition(
                new WorkflowStepId(entity.Id),
                new WorkflowVersionId(entity.WorkflowVersionId),
                entity.Key,
                entity.Name,
                (WorkflowStepKind)entity.Kind,
                entity.IsStart,
                entity.IsTerminal,
                new WorkflowRetryPolicy(
                    entity.RetryMaxAttempts,
                    TimeSpan.FromMilliseconds(entity.RetryDelayMilliseconds)),
                new WorkflowTimeoutPolicy(TimeSpan.FromMilliseconds(entity.TimeoutMilliseconds)),
                string.IsNullOrWhiteSpace(entity.CompensationOperation)
                    ? null
                    : new WorkflowCompensationHook(entity.CompensationOperation)))
            .ToList();
    }

    public IReadOnlyList<WorkflowTransitionDefinition> GetAllTransitions()
    {
        var entities = _dbContext
            .Set<PlatformWorkflowTransitionEntity>()
            .AsNoTracking()
            .OrderBy(x => x.WorkflowVersionId)
            .ThenBy(x => x.FromStepId)
            .ThenBy(x => x.Priority)
            .ToList();

        return entities
            .Select(entity => new WorkflowTransitionDefinition(
                new WorkflowTransitionId(entity.Id),
                new WorkflowVersionId(entity.WorkflowVersionId),
                new WorkflowStepId(entity.FromStepId),
                new WorkflowStepId(entity.ToStepId),
                (WorkflowBranchKind)entity.BranchKind,
                new WorkflowPriority(entity.Priority),
                (WorkflowTransitionConditionKind)entity.ConditionKind,
                entity.RuleSetKey,
                entity.RuleScopeKind.HasValue
                    ? WorkflowScope.Create((WorkflowScopeKind)entity.RuleScopeKind.Value, entity.RuleScopeIdentifier)
                    : null))
            .ToList();
    }
}
