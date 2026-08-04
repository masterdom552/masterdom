using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents signed adjustment line.
/// </summary>
public sealed class AdjustmentLine : ValueObject
{
    private AdjustmentLine(AdjustmentKind kind, string description, decimal amount)
    {
        Kind = kind;
        Description = description;
        Amount = amount;
    }

    public AdjustmentKind Kind { get; }

    public string Description { get; }

    public decimal Amount { get; }

    public static AdjustmentLine Create(AdjustmentKind kind, string description, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var normalized = description.Trim();
        if (normalized.Length > 300)
        {
            throw new InvalidOperationException("Adjustment description cannot exceed 300 characters.");
        }

        return new AdjustmentLine(kind, normalized, amount);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return Description;
        yield return Amount;
    }
}
