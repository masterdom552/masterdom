using Masterdom.Core.Identifiers;
using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Events;

public interface IBillingApplicationEvent
{
    string CorrelationId { get; }

    BillingPeriod BillingPeriod { get; }

    IReadOnlyCollection<BillId> PersistedBillIds { get; }

    int PersistedBillCount { get; }

    DateTime ExecutionTimestampUtc { get; }

    PropertyReference? PropertyReference { get; }
}
