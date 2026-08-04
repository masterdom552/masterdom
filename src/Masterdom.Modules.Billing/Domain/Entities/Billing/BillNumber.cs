using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents bill business number.
/// </summary>
public sealed class BillNumber : ValueObject
{
    private BillNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BillNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length > 50)
        {
            throw new ArgumentException("Bill number cannot exceed 50 characters.", nameof(value));
        }

        return new BillNumber(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
