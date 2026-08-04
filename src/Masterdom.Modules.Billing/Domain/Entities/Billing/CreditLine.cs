using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents credit line applied to a bill.
/// </summary>
public sealed class CreditLine : ValueObject
{
    private CreditLine(string description, decimal amount, string? sourceReference)
    {
        Description = description;
        Amount = amount;
        SourceReference = sourceReference;
    }

    public string Description { get; }

    public decimal Amount { get; }

    public string? SourceReference { get; }

    public static CreditLine Create(string description, decimal amount, string? sourceReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (amount <= 0)
        {
            throw new InvalidOperationException("Credit amount must be greater than zero.");
        }

        var normalizedDescription = description.Trim();
        if (normalizedDescription.Length > 300)
        {
            throw new InvalidOperationException("Credit description cannot exceed 300 characters.");
        }

        var normalizedReference = string.IsNullOrWhiteSpace(sourceReference) ? null : sourceReference.Trim();
        if (normalizedReference is not null && normalizedReference.Length > 150)
        {
            throw new InvalidOperationException("Credit source reference cannot exceed 150 characters.");
        }

        return new CreditLine(normalizedDescription, amount, normalizedReference);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Description;
        yield return Amount;
        yield return SourceReference;
    }
}
