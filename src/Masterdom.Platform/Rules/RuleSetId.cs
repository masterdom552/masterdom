using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents the unique identity of a rule set.
/// </summary>
public readonly struct RuleSetId
{
    public RuleSetId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new RuleValidationException("RuleSetId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
