namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents rule priority where lower numbers execute earlier.
/// </summary>
public readonly struct RulePriority
{
    public RulePriority(int value)
    {
        if (value < 1 || value > 1000)
        {
            throw new RuleValidationException(
                "Rule priority must be between 1 and 1000.");
        }

        Value = value;
    }

    public int Value { get; }
}
