using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a normalized rule-set key.
/// </summary>
public sealed class RuleSetKey : IEquatable<RuleSetKey>
{
    public RuleSetKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RuleValidationException("Rule set key is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(RuleSetKey? other)
    {
        return other is not null &&
               string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is RuleSetKey other && Equals(other);
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
