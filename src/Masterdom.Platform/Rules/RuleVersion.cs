namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a rule or rule-set version number.
/// </summary>
public readonly struct RuleVersion
{
    public RuleVersion(int value)
    {
        if (value <= 0)
        {
            throw new RuleValidationException("Rule version must be greater than zero.");
        }

        Value = value;
    }

    public int Value { get; }
}
