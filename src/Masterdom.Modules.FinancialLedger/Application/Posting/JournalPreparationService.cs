using Masterdom.Modules.FinancialLedger.Application.Translation;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class JournalPreparationService
{
    private readonly IJournalNumberGenerator _journalNumberGenerator;

    public JournalPreparationService(IJournalNumberGenerator journalNumberGenerator)
    {
        _journalNumberGenerator = journalNumberGenerator ?? throw new ArgumentNullException(nameof(journalNumberGenerator));
    }

    public PreparedJournal Prepare(
        BillingSnapshotPostingSourceModel source,
        PostingLineGenerationResult generatedLines,
        DateOnly postingDate,
        string? batchReference = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(generatedLines);

        var resolvedBatchReference = string.IsNullOrWhiteSpace(batchReference)
            ? $"BILL-{source.BillingPeriodStartDate:yyyyMM}"
            : batchReference.Trim();

        var postingReference = $"BILL:{source.BillId:N}";
        var journalNumber = _journalNumberGenerator.Generate("billing", postingDate, postingReference);
        var journalReference = $"PREP-{source.BillNumber}-{postingDate:yyyyMMdd}";

        var lines = generatedLines.Lines
            .Select(x => new PreparedJournalLine(
                x.LineId,
                x.AccountCode,
                x.AccountName,
                x.Direction,
                x.Amount,
                x.CurrencyCode,
                x.Description,
                x.Metadata))
            .ToList()
            .AsReadOnly();

        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["billId"] = source.BillId.ToString("N"),
            ["billNumber"] = source.BillNumber,
            ["billingPeriodStartDate"] = source.BillingPeriodStartDate.ToString("yyyy-MM-dd"),
            ["billingPeriodEndDate"] = source.BillingPeriodEndDate.ToString("yyyy-MM-dd"),
            ["propertyId"] = source.PropertyId.ToString("N"),
            ["tenancyId"] = source.TenancyId.ToString("N"),
            ["leaseId"] = source.LeaseId.ToString("N")
        };

        if (!string.IsNullOrWhiteSpace(source.CorrelationId))
        {
            metadata["correlationId"] = source.CorrelationId.Trim();
        }

        return new PreparedJournal(
            journalReference,
            postingReference,
            journalNumber,
            postingDate,
            source.CurrencyCode,
            $"Prepared journal for bill {source.BillNumber}",
            resolvedBatchReference,
            "billing",
            source.BillId,
            source.BillNumber,
            lines,
            metadata: metadata);
    }
}
