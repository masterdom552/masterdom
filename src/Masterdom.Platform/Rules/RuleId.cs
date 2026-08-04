using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents the unique identity of a rule definition.
/// </summary>
public readonly struct RuleId
{
    public RuleId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new RuleValidationException("RuleId cannot be empty.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
