using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyType : ValueObject
{
    private PolicyType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PolicyType Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new PolicyType(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
