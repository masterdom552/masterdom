using System;

namespace Masterdom.Platform.Rules;

/// <summary>
/// Represents the scope target of rules and rule sets.
/// </summary>
public sealed class RuleScope : IEquatable<RuleScope>
{
    private RuleScope(RuleScopeKind kind, string? identifier)
    {
        Kind = kind;
        Identifier = identifier;
    }

    public RuleScopeKind Kind { get; }

    public string? Identifier { get; }

    public static RuleScope Global()
    {
        return new RuleScope(RuleScopeKind.Global, null);
    }

    public static RuleScope Create(RuleScopeKind kind, string? identifier)
    {
        if (kind == RuleScopeKind.Global)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                throw new RuleValidationException(
                    "Global rule scope cannot contain an identifier.");
            }

            return Global();
        }

        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new RuleValidationException(
                $"Rule scope identifier is required for scope '{kind}'.");
        }

        return new RuleScope(kind, identifier.Trim());
    }

    public bool Equals(RuleScope? other)
    {
        return other is not null &&
               Kind == other.Kind &&
               string.Equals(Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return obj is RuleScope other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Kind, Identifier?.ToUpperInvariant());
    }

    public override string ToString()
    {
        return Identifier is null ? Kind.ToString() : $"{Kind}:{Identifier}";
    }
}
