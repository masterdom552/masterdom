using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Contracts.Billing;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class BillingSnapshotPostingPreparationResult
{
    public BillingSnapshotPostingPreparationResult(
        FinancialPostingRequest postingRequest,
        BillingLedgerPostingContract legacyContract,
        PostingLineGenerationResult generatedLines,
        PreparedJournal preparedJournal)
    {
        PostingRequest = postingRequest ?? throw new ArgumentNullException(nameof(postingRequest));
        LegacyContract = legacyContract ?? throw new ArgumentNullException(nameof(legacyContract));
        GeneratedLines = generatedLines ?? throw new ArgumentNullException(nameof(generatedLines));
        PreparedJournal = preparedJournal ?? throw new ArgumentNullException(nameof(preparedJournal));
    }

    public FinancialPostingRequest PostingRequest { get; }

    public BillingLedgerPostingContract LegacyContract { get; }

    public PostingLineGenerationResult GeneratedLines { get; }

    public PreparedJournal PreparedJournal { get; }
}
