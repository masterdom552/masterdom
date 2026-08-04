using Masterdom.Core.Primitives;
using Masterdom.Core.Financial.ValueObjects;

namespace Masterdom.Modules.Billing.Domain.Entities.Billing;

/// <summary>
/// Represents immutable billing snapshot.
/// </summary>
public sealed class BillSnapshot : ValueObject
{
    private BillSnapshot(
        SnapshotVersion version,
        BillingPeriod billingPeriod,
        BillingCycle billingCycle,
        GeneratedDate generatedDate,
        IssueDate issueDate,
        DueDate dueDate,
        Currency currency,
        ChargeCollection charges,
        AdjustmentCollection adjustments,
        CreditCollection credits,
        TotalAmount totalAmount,
        OutstandingAmount outstandingAmount)
    {
        Version = version;
        BillingPeriod = billingPeriod;
        BillingCycle = billingCycle;
        GeneratedDate = generatedDate;
        IssueDate = issueDate;
        DueDate = dueDate;
        Currency = currency;
        Charges = charges;
        Adjustments = adjustments;
        Credits = credits;
        TotalAmount = totalAmount;
        OutstandingAmount = outstandingAmount;
    }

    public SnapshotVersion Version { get; }

    public BillingPeriod BillingPeriod { get; }

    public BillingCycle BillingCycle { get; }

    public GeneratedDate GeneratedDate { get; }

    public IssueDate IssueDate { get; }

    public DueDate DueDate { get; }

    public Currency Currency { get; }

    public ChargeCollection Charges { get; }

    public AdjustmentCollection Adjustments { get; }

    public CreditCollection Credits { get; }

    public TotalAmount TotalAmount { get; }

    public OutstandingAmount OutstandingAmount { get; }

    public static BillSnapshot Create(
        SnapshotVersion version,
        BillingPeriod billingPeriod,
        BillingCycle billingCycle,
        GeneratedDate generatedDate,
        IssueDate issueDate,
        DueDate dueDate,
        Currency currency,
        ChargeCollection charges,
        AdjustmentCollection adjustments,
        CreditCollection credits)
    {
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(billingPeriod);
        ArgumentNullException.ThrowIfNull(billingCycle);
        ArgumentNullException.ThrowIfNull(generatedDate);
        ArgumentNullException.ThrowIfNull(issueDate);
        ArgumentNullException.ThrowIfNull(dueDate);
        ArgumentNullException.ThrowIfNull(currency);
        ArgumentNullException.ThrowIfNull(charges);
        ArgumentNullException.ThrowIfNull(adjustments);
        ArgumentNullException.ThrowIfNull(credits);

        if (dueDate.Value < issueDate.Value)
        {
            throw new InvalidOperationException("Due date cannot be earlier than issue date.");
        }

        var total = charges.TotalAmount + adjustments.TotalAmount - credits.TotalAmount;
        if (total < 0)
        {
            throw new InvalidOperationException("Bill total cannot be negative.");
        }

        return new BillSnapshot(
            version,
            billingPeriod,
            billingCycle,
            generatedDate,
            issueDate,
            dueDate,
            currency,
            charges,
            adjustments,
            credits,
            TotalAmount.Create(total),
            OutstandingAmount.Create(total));
    }

    public BillSnapshot RecalculateWith(
        AdjustmentCollection adjustments,
        CreditCollection credits,
        SnapshotVersion nextVersion,
        GeneratedDate generatedDate,
        IssueDate issueDate,
        DueDate dueDate)
    {
        return Create(
            nextVersion,
            BillingPeriod,
            BillingCycle,
            generatedDate,
            issueDate,
            dueDate,
            Currency,
            Charges,
            adjustments,
            credits);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Version;
        yield return BillingPeriod;
        yield return BillingCycle;
        yield return GeneratedDate;
        yield return IssueDate;
        yield return DueDate;
        yield return Currency;
        yield return Charges;
        yield return Adjustments;
        yield return Credits;
        yield return TotalAmount;
        yield return OutstandingAmount;
    }
}
