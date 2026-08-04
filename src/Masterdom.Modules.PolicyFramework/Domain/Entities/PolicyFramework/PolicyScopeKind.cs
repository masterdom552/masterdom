using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyScopeKind : ValueObject
{
    public static readonly PolicyScopeKind Global = new("Global");
    public static readonly PolicyScopeKind Module = new("Module");
    public static readonly PolicyScopeKind Tenant = new("Tenant");
    public static readonly PolicyScopeKind Property = new("Property");

    private PolicyScopeKind(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PolicyScopeKind Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        return value.Trim().ToUpperInvariant() switch
        {
            "GLOBAL" => Global,
            "MODULE" => Module,
            "TENANT" => Tenant,
            "PROPERTY" => Property,
            _ => new PolicyScopeKind(value.Trim())
        };
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
