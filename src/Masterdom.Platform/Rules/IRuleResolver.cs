namespace Masterdom.Platform.Rules;

/// <summary>
/// Resolves and evaluates effective rule sets.
/// </summary>
public interface IRuleResolver
{
    RuleOutput Evaluate(
        RuleSetKey ruleSetKey,
        RuleScope scope,
        RuleContext context,
        RuleInput input);
}
