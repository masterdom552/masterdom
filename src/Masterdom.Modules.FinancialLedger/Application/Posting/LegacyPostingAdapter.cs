using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Translation;
using Masterdom.Modules.FinancialLedger.Contracts.Billing;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class LegacyPostingAdapter
{
    public BillingLedgerPostingContract Adapt(
        BillingSnapshotPostingSourceModel source,
        PostingLineGenerationResult generatedLines,
        DateOnly postingDate,
        string? batchReference = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(generatedLines);

        var postingReference = $"BILL:{source.BillId:N}";
        var journalNumber = $"JRN-{source.BillNumber}";
        var resolvedBatchReference = string.IsNullOrWhiteSpace(batchReference)
            ? $"BILL-{source.BillingPeriodStartDate:yyyyMM}"
            : batchReference.Trim();

        var lines = generatedLines.Lines
            .Select(x => new LedgerPostingLineContract(
                x.AccountCode,
                x.AccountName,
                x.Direction == FinancialPostingDirection.Debit ? x.Amount : 0m,
                x.Direction == FinancialPostingDirection.Credit ? x.Amount : 0m,
                x.Description))
            .ToList()
            .AsReadOnly();

        return new BillingLedgerPostingContract(
            postingReference,
            journalNumber,
            postingDate,
            $"Bill {source.BillNumber} posting",
            resolvedBatchReference,
            lines);
    }
}
