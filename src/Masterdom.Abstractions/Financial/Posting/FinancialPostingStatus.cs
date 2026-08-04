namespace Masterdom.Abstractions.Financial.Posting;

public enum FinancialPostingStatus
{
    Unspecified = 0,
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Posted = 4,
    Failed = 5,
    Cancelled = 6,
    Reversed = 7
}
