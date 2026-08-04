namespace Masterdom.Platform.Rules;

/// <summary>
/// Defines supported rule kinds.
/// </summary>
public enum RuleKind
{
    Boolean = 0,
    Comparison = 1,
    Range = 2,
    Expression = 3,
    Composite = 4
}
