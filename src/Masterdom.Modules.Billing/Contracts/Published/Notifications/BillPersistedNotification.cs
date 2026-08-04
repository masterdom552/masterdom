namespace Masterdom.Modules.Billing.Contracts.Published.Notifications;

public sealed class BillPersistedNotification
{
    public BillPersistedNotification(
        string correlationId,
        DateOnly billingPeriodStartDate,
        DateOnly billingPeriodEndDate,
        IReadOnlyCollection<Guid> persistedBillIds,
        int persistedBillCount,
        DateTime executionTimestampUtc,
        Guid? propertyId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        ArgumentNullException.ThrowIfNull(persistedBillIds);

        var billIds = persistedBillIds.ToList();
        if (billIds.Count == 0)
        {
            throw new ArgumentException("At least one persisted bill id is required.", nameof(persistedBillIds));
        }

        if (persistedBillCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(persistedBillCount), "Persisted bill count must be greater than zero.");
        }

        if (billIds.Count != persistedBillCount)
        {
            throw new ArgumentException("Persisted bill count must match the number of persisted bill ids.", nameof(persistedBillCount));
        }

        if (billingPeriodEndDate < billingPeriodStartDate)
        {
            throw new ArgumentException("Billing period end date must be on or after the start date.", nameof(billingPeriodEndDate));
        }

        if (executionTimestampUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Execution timestamp must be UTC.", nameof(executionTimestampUtc));
        }

        CorrelationId = correlationId.Trim();
        BillingPeriodStartDate = billingPeriodStartDate;
        BillingPeriodEndDate = billingPeriodEndDate;
        PersistedBillIds = billIds.AsReadOnly();
        PersistedBillCount = persistedBillCount;
        ExecutionTimestampUtc = executionTimestampUtc;
        PropertyId = propertyId;
    }

    public string CorrelationId { get; }

    public DateOnly BillingPeriodStartDate { get; }

    public DateOnly BillingPeriodEndDate { get; }

    public IReadOnlyCollection<Guid> PersistedBillIds { get; }

    public int PersistedBillCount { get; }

    public DateTime ExecutionTimestampUtc { get; }

    public Guid? PropertyId { get; }
}
