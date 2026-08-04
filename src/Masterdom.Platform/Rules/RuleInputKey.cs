using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a normalized key used to access a rule input.
/// </summary>
public sealed class RuleInputKey : IEquatable<RuleInputKey>
{
    public RuleInputKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RuleValidationException("Rule input key is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(RuleInputKey? other)
    {
        return other is not null &&
               string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is RuleInputKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
    }

    public override string ToString()
    {
        return Value;
    }
}
