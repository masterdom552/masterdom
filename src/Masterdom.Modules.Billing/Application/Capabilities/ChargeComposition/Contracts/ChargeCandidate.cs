using System.Collections.ObjectModel;

namespace Masterdom.Modules.Billing.Application.Capabilities.ChargeComposition.Contracts;

public sealed class ChargeCandidate
{
    public ChargeCandidate(
        string chargeType,
        string description,
        decimal amount,
        string currency,
        string sourceCapability,
        string? externalReference = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chargeType);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCapability);

        if (amount < 0)
        {
            throw new InvalidOperationException("Charge candidate amount cannot be negative.");
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (normalizedCurrency.Length != 3)
        {
            throw new InvalidOperationException("Currency must use ISO-4217 alpha-3 format.");
        }

        var materializedMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (metadata is not null)
        {
            foreach (var entry in metadata)
            {
                if (string.IsNullOrWhiteSpace(entry.Key))
                {
                    throw new InvalidOperationException("Charge candidate metadata key cannot be empty.");
                }

                if (string.IsNullOrWhiteSpace(entry.Value))
                {
                    throw new InvalidOperationException("Charge candidate metadata value cannot be empty.");
                }

                materializedMetadata[entry.Key.Trim()] = entry.Value.Trim();
            }
        }

        ChargeType = chargeType.Trim();
        Description = description.Trim();
        Amount = amount;
        Currency = normalizedCurrency;
        SourceCapability = sourceCapability.Trim();
        ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        Metadata = new ReadOnlyDictionary<string, string>(materializedMetadata);
    }

    public string ChargeType { get; }

    public string Description { get; }

    public decimal Amount { get; }

    public string Currency { get; }

    public string SourceCapability { get; }

    public string? ExternalReference { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
