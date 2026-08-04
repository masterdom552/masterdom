namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal enum JournalLifecycleState
{
    Prepared = 0,
    Validated = 1,
    Posted = 2,
    Reversed = 3,
    Cancelled = 4
}
