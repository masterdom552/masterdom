namespace Masterdom.Platform.Rules;

/// <summary>
/// Defines comparison operators for rule conditions.
/// </summary>
public enum RuleComparisonOperator
{
    Equal = 0,
    NotEqual = 1,
    GreaterThan = 2,
    GreaterThanOrEqual = 3,
    LessThan = 4,
    LessThanOrEqual = 5,
    Contains = 6,
    StartsWith = 7,
    EndsWith = 8
}
