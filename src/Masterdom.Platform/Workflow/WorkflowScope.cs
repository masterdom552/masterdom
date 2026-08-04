using System;

namespace Masterdom.Platform.Workflow;

/// <summary>
/// Represents workflow scope target.
/// </summary>
public sealed class WorkflowScope : IEquatable<WorkflowScope>
{
    private WorkflowScope(WorkflowScopeKind kind, string? identifier)
    {
        Kind = kind;
        Identifier = identifier;
    }

    public WorkflowScopeKind Kind { get; }

    public string? Identifier { get; }

    public static WorkflowScope Global()
    {
        return new WorkflowScope(WorkflowScopeKind.Global, null);
    }

    public static WorkflowScope Create(WorkflowScopeKind kind, string? identifier)
    {
        if (kind == WorkflowScopeKind.Global)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                throw new WorkflowValidationException("Global workflow scope cannot include an identifier.");
            }

            return Global();
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new WorkflowValidationException($"Workflow scope identifier is required for scope '{kind}'.");
        }

        return new WorkflowScope(kind, identifier.Trim());
    }

    public bool Equals(WorkflowScope? other)
    {
        return other is not null &&
               Kind == other.Kind &&
               string.Equals(Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is WorkflowScope other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, Identifier?.ToUpperInvariant());
    }
}
