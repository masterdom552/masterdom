namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents the result of evaluating one rule.
/// </summary>
public sealed class RuleResult
{
    public required RuleId RuleId { get; init; }

    public required RuleKey RuleKey { get; init; }

    public required RuleResultStatus Status { get; init; }

    public required RuleSeverity Severity { get; init; }

    public required RulePriority Priority { get; init; }

    public required string Message { get; init; }
}
