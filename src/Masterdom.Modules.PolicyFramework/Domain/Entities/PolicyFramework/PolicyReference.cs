using Masterdom.Core.Primitives;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

public sealed class PolicyReference : ValueObject
{
    private PolicyReference(string policyCode, string displayName)
    {
        PolicyCode = policyCode;
        DisplayName = displayName;
    }

    public string PolicyCode { get; }

    public string DisplayName { get; }

    public static PolicyReference Create(string policyCode, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        return new PolicyReference(policyCode.Trim(), displayName.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PolicyCode.ToUpperInvariant();
        yield return DisplayName.ToUpperInvariant();
    }
}
