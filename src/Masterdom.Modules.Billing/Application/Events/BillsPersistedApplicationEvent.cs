using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Events;

public sealed class BillsPersistedApplicationEvent : IBillingApplicationEvent
{
    public BillsPersistedApplicationEvent(
        string correlationId,
        BillingPeriod billingPeriod,
        IReadOnlyCollection<BillId> persistedBillIds,
        int persistedBillCount,
        DateTime executionTimestampUtc,
        PropertyReference? propertyReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        ArgumentNullException.ThrowIfNull(billingPeriod);
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

        if (executionTimestampUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Execution timestamp must be UTC.", nameof(executionTimestampUtc));
        }

        CorrelationId = correlationId.Trim();
        BillingPeriod = billingPeriod;
        PersistedBillIds = billIds.AsReadOnly();
        PersistedBillCount = persistedBillCount;
        ExecutionTimestampUtc = executionTimestampUtc;
        PropertyReference = propertyReference;
    }

    public string CorrelationId { get; }

    public BillingPeriod BillingPeriod { get; }

    public IReadOnlyCollection<BillId> PersistedBillIds { get; }

    public int PersistedBillCount { get; }

    public DateTime ExecutionTimestampUtc { get; }

    public PropertyReference? PropertyReference { get; }
}
