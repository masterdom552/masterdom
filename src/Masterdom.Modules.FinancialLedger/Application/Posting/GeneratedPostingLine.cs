using System.Collections.ObjectModel;
using Masterdom.Abstractions.Financial.Posting;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class GeneratedPostingLine
{
    public GeneratedPostingLine(
        string lineId,
        string accountCode,
        string accountName,
        FinancialPostingDirection direction,
        decimal amount,
        string currencyCode,
        string description,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lineId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Posting amount must be greater than zero.");
        }

        LineId = lineId.Trim();
        AccountCode = accountCode.Trim().ToUpperInvariant();
        AccountName = accountName.Trim();
        Direction = direction;
        Amount = amount;
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        Description = description.Trim();

        var materialized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (metadata is not null)
        {
            foreach (var entry in metadata)
            {
                materialized[entry.Key] = entry.Value;
            }
        }

        Metadata = new ReadOnlyDictionary<string, string>(materialized);
    }

    public string LineId { get; }

    public string AccountCode { get; }

    public string AccountName { get; }

    public FinancialPostingDirection Direction { get; }

    public decimal Amount { get; }

    public string CurrencyCode { get; }

    public string Description { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }
}
