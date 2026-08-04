using System;
using System.Collections.Generic;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// In-memory workflow state persistence.
/// </summary>
public sealed class InMemoryWorkflowStateStore : IWorkflowStateStore
{
    private readonly Dictionary<Guid, WorkflowState> _states = new();

    public void Save(WorkflowState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        _states[state.InstanceId] = state;
    }

    public bool TryGet(Guid instanceId, out WorkflowState? state)
    {
        var found = _states.TryGetValue(instanceId, out var value);
        state = found ? value : null;
        return found;
    }
}
