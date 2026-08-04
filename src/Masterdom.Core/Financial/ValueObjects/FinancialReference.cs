using Masterdom.Core.Primitives;

namespace Masterdom.Core.Financial.ValueObjects;

public sealed class FinancialReference : ValueObject
{
    public string ReferenceType { get; }

    public string ReferenceValue { get; }

    private FinancialReference(string referenceType, string referenceValue)
    {
        ReferenceType = referenceType;
        ReferenceValue = referenceValue;
    }

    public static FinancialReference Create(string referenceType, string referenceValue)
    {
        if (string.IsNullOrWhiteSpace(referenceType))
            throw new ArgumentException("Reference type is required.", nameof(referenceType));

        if (string.IsNullOrWhiteSpace(referenceValue))
            throw new ArgumentException("Reference value is required.", nameof(referenceValue));

        return new FinancialReference(referenceType.Trim(), referenceValue.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ReferenceType;
        yield return ReferenceValue;
    }
}
