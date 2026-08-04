using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyCondition : ValueObject
{
    private PolicyCondition(string selectorKey, string selectorDefinition)
    {
        SelectorKey = selectorKey;
        SelectorDefinition = selectorDefinition;
    }

    public string SelectorKey { get; }

    public string SelectorDefinition { get; }

    public static PolicyCondition Create(string selectorKey, string selectorDefinition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectorKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectorDefinition);

        return new PolicyCondition(selectorKey.Trim(), selectorDefinition.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SelectorKey.ToUpperInvariant();
        yield return SelectorDefinition;
    }
}
