using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents a normalized rule key.
/// </summary>
public sealed class RuleKey : IEquatable<RuleKey>
{
    public RuleKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RuleValidationException("Rule key is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public bool Equals(RuleKey? other)
    {
        return other is not null &&
               string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is RuleKey other && Equals(other);
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
