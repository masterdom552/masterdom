using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyCategory : ValueObject
{
    private PolicyCategory(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PolicyCategory Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new PolicyCategory(value.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }
}
