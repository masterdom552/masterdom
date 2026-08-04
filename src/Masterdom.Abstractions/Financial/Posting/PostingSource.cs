namespace Masterdom.Abstractions.Financial.Posting;

public enum PostingSource
{
    Unspecified = 0,
    Billing = 1,
    Payments = 2,
    Deposits = 3,
    Refunds = 4,
    Penalties = 5,
    Maintenance = 6,
    Inventory = 7,
    Manual = 8,
    External = 9
}
