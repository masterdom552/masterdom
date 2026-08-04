namespace Masterdom.Abstractions.Financial.Posting;

public enum FinancialTransactionType
{
    Unspecified = 0,
    Charge = 1,
    Payment = 2,
    Deposit = 3,
    Refund = 4,
    Penalty = 5,
    Adjustment = 6,
    Reversal = 7,
    Transfer = 8,
    Other = 9
}
