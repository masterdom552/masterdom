using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyScope : ValueObject
{
    private PolicyScope(PolicyScopeKind kind, string scopeKey)
    {
        Kind = kind;
        ScopeKey = scopeKey;
    }

    public PolicyScopeKind Kind { get; }

    public string ScopeKey { get; }

    public static PolicyScope Create(PolicyScopeKind kind, string scopeKey)
    {
        ArgumentNullException.ThrowIfNull(kind);

        if (kind == PolicyScopeKind.Global)
        {
            return new PolicyScope(kind, "GLOBAL");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(scopeKey);
        return new PolicyScope(kind, scopeKey.Trim());
    }

    public bool AppliesTo(PolicyScope requestedScope)
    {
        ArgumentNullException.ThrowIfNull(requestedScope);

        if (Kind == PolicyScopeKind.Global)
        {
            return true;
        }

        return Kind == requestedScope.Kind
            && string.Equals(ScopeKey, requestedScope.ScopeKey, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return ScopeKey.ToUpperInvariant();
    }
}
