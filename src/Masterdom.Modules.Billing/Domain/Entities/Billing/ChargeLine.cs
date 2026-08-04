using Masterdom.Core.Primitives;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents an obligation charge line.
/// </summary>
public sealed class ChargeLine : ValueObject
{
    private ChargeLine(ChargeKind kind, string description, decimal amount, string? externalReference)
    {
        Kind = kind;
        Description = description;
        Amount = amount;
        ExternalReference = externalReference;
    }

    public ChargeKind Kind { get; }

    public string Description { get; }

    public decimal Amount { get; }

    public string? ExternalReference { get; }

    public static ChargeLine Create(ChargeKind kind, string description, decimal amount, string? externalReference = null)
    {
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (amount < 0)
        {
            throw new InvalidOperationException("Charge amount cannot be negative.");
        }

        var normalizedDescription = description.Trim();
        if (normalizedDescription.Length > 300)
        {
            throw new InvalidOperationException("Charge description cannot exceed 300 characters.");
        }

        var normalizedReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        if (normalizedReference is not null && normalizedReference.Length > 150)
        {
            throw new InvalidOperationException("External reference cannot exceed 150 characters.");
        }

        return new ChargeLine(kind, normalizedDescription, amount, normalizedReference);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return Description;
        yield return Amount;
        yield return ExternalReference;
    }
}
