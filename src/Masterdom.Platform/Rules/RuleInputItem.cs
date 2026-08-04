namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a single typed input item for rule evaluation.
/// </summary>
public sealed class RuleInputItem
{
    public required RuleInputKey Key { get; init; }

    public required RuleValue Value { get; init; }
}
