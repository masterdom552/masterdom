using System;
using System.Collections.Generic;
using System.Linq;
using Masterdom.Platform.Configuration;
using Masterdom.Platform.Metadata;
using Masterdom.Platform.Rules;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Resolves and executes effective workflows.
/// </summary>
public sealed class WorkflowResolver : IWorkflowResolver
{
    private readonly IWorkflowRepository _repository;
    private readonly IConfigurationResolver _configuration;
    private readonly IMetadataResolver _metadata;
    private readonly IRuleResolver _rules;
    private readonly IWorkflowStateStore _stateStore;

    public WorkflowResolver(
        IWorkflowRepository repository,
        IConfigurationResolver configuration,
        IMetadataResolver metadata,
        IRuleResolver rules,
        IWorkflowStateStore stateStore)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        _stateStore = stateStore ?? throw new ArgumentNullException(nameof(stateStore));
    }

    public WorkflowResult Execute(WorkflowKey workflowKey, WorkflowScope scope, WorkflowContext context)
    {
        ArgumentNullException.ThrowIfNull(workflowKey);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(context.ModuleId))
        {
            throw new WorkflowValidationException("ModuleId is required for workflow context.");
        }

        if (context.AsOfUtc.Kind != DateTimeKind.Utc)
        {
            throw new WorkflowValidationException("Workflow execution timestamp must be UTC.");
        }

        _ = _configuration;
        _ = _metadata;

        var workflows = _repository.GetAllWorkflows();
        var versions = _repository.GetAllVersions();
        var steps = _repository.GetAllSteps();
        var transitions = _repository.GetAllTransitions();

        WorkflowValidation.ValidateAll(workflows, versions, steps, transitions);

        var workflow = workflows.FirstOrDefault(x => x.Key.Equals(workflowKey) && x.Scope.Equals(scope));
        if (workflow is null)
        {
            throw new WorkflowValidationException("Workflow was not found for key and scope.");
        }

        var activeVersion = versions
            .Where(x => x.WorkflowId.Value == workflow.Id.Value)
            .Where(x => x.Period.IsEffectiveAt(context.AsOfUtc))
            .OrderByDescending(x => x.Period.EffectiveFromUtc)
            .ThenByDescending(x => x.Version.Value)
            .FirstOrDefault();

        if (activeVersion is null)
        {
            throw new WorkflowValidationException("No active workflow version was found.");
        }

        var vSteps = steps
            .Where(x => x.WorkflowVersionId.Value == activeVersion.Id.Value)
            .ToDictionary(x => x.Id.Value);

        var vTransitions = transitions
            .Where(x => x.WorkflowVersionId.Value == activeVersion.Id.Value)
            .GroupBy(x => x.FromStepId.Value)
            .ToDictionary(x => x.Key, x => x.OrderBy(t => t.Priority.Value).ToList());

        var start = vSteps.Values.Single(x => x.IsStart);

        var state = new WorkflowState
        {
            InstanceId = Guid.NewGuid(),
            WorkflowId = workflow.Id,
            WorkflowVersionId = activeVersion.Id,
            CurrentStepId = start.Id,
            Status = WorkflowExecutionStatus.Running,
            CompletedSteps = new List<WorkflowStepId>(),
            PendingSteps = new List<WorkflowStepId>(),
            History = new List<WorkflowExecutionEvent>(),
            StartedAtUtc = context.AsOfUtc
        };

        state.History.Add(new WorkflowExecutionEvent
        {
            TimestampUtc = context.AsOfUtc,
            EventType = "Started",
            StepId = start.Id,
            Message = "Workflow execution started."
        });

        if (context.CancellationRequested)
        {
            state.Status = WorkflowExecutionStatus.Cancelled;
            state.CompletedAtUtc = context.AsOfUtc;
            state.History.Add(new WorkflowExecutionEvent
            {
                TimestampUtc = context.AsOfUtc,
                EventType = "Cancelled",
                Message = "Cancellation was requested before execution."
            });

            _stateStore.Save(state);
            return new WorkflowResult { State = state };
        }

        var cursor = start;
        while (true)
        {
            if (cursor.Kind == WorkflowStepKind.ManualApproval)
            {
                state.PendingSteps.Add(cursor.Id);
                state.CurrentStepId = cursor.Id;
                state.History.Add(new WorkflowExecutionEvent
                {
                    TimestampUtc = context.AsOfUtc,
                    EventType = "ManualApprovalPending",
                    StepId = cursor.Id,
                    Message = "Manual approval is required."
                });

                _stateStore.Save(state);
                return new WorkflowResult { State = state };
            }

            state.CompletedSteps.Add(cursor.Id);
            state.History.Add(new WorkflowExecutionEvent
            {
                TimestampUtc = context.AsOfUtc,
                EventType = "StepCompleted",
                StepId = cursor.Id,
                Message = "Automatic step completed."
            });

            if (cursor.IsTerminal)
            {
                state.Status = WorkflowExecutionStatus.Completed;
                state.CurrentStepId = null;
                state.CompletedAtUtc = context.AsOfUtc;
                state.History.Add(new WorkflowExecutionEvent
                {
                    TimestampUtc = context.AsOfUtc,
                    EventType = "Completed",
                    Message = "Workflow execution completed."
                });

                _stateStore.Save(state);
                return new WorkflowResult { State = state };
            }

            if (!vTransitions.TryGetValue(cursor.Id.Value, out var outgoing) || outgoing.Count == 0)
            {
                state.Status = WorkflowExecutionStatus.Failed;
                state.Error = "No outgoing transition was found from current step.";
                state.CompletedAtUtc = context.AsOfUtc;
                _stateStore.Save(state);
                return new WorkflowResult { State = state };
            }

            var eligible = outgoing.Where(x => IsTransitionAllowed(x, scope, context)).ToList();
            if (eligible.Count == 0)
            {
                state.Status = WorkflowExecutionStatus.Failed;
                state.Error = "No eligible transition was found from current step.";
                state.CompletedAtUtc = context.AsOfUtc;
                _stateStore.Save(state);
                return new WorkflowResult { State = state };
            }

            var parallel = eligible.Where(x => x.BranchKind == WorkflowBranchKind.Parallel).ToList();
            if (parallel.Count > 0)
            {
                foreach (var branch in parallel)
                {
                    state.PendingSteps.Add(branch.ToStepId);
                }

                state.History.Add(new WorkflowExecutionEvent
                {
                    TimestampUtc = context.AsOfUtc,
                    EventType = "ParallelScheduled",
                    StepId = cursor.Id,
                    Message = $"Parallel branches scheduled: {parallel.Count}."
                });
            }

            var next = eligible[0];
            if (!vSteps.TryGetValue(next.ToStepId.Value, out cursor))
            {
                state.Status = WorkflowExecutionStatus.Failed;
                state.Error = "Transition target step was not found.";
                state.CompletedAtUtc = context.AsOfUtc;
                _stateStore.Save(state);
                return new WorkflowResult { State = state };
            }

            state.CurrentStepId = cursor.Id;
        }
    }

    private bool IsTransitionAllowed(
        WorkflowTransitionDefinition transition,
        WorkflowScope workflowScope,
        WorkflowContext context)
    {
        if (transition.ConditionKind == WorkflowTransitionConditionKind.Always)
        {
            return true;
        }

        var ruleScope = transition.RuleScope ?? workflowScope;
        var ruleOutput = _rules.Evaluate(
            new RuleSetKey(transition.RuleSetKey!.Value),
            RuleScope.Create((RuleScopeKind)ruleScope.Kind, ruleScope.Identifier),
            new RuleContext
            {
                ModuleId = context.ModuleId,
                TenantId = context.TenantId,
                PropertyId = context.PropertyId,
                CorrelationId = context.CorrelationId,
                AsOfUtc = context.AsOfUtc
            },
            context.Input);

        return ruleOutput.Passed;
    }
}
