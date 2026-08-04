using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents a normalized workflow key.
/// </summary>
public sealed class WorkflowKey : IEquatable<WorkflowKey>
{
    public WorkflowKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new WorkflowValidationException("Workflow key is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(WorkflowKey? other)
    {
        return other is not null &&
               string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is WorkflowKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
    }
}
