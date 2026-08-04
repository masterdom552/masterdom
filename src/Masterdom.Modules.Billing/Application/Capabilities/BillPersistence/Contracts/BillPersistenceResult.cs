using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;

public sealed class BillPersistenceResult
{
    public BillPersistenceResult(IReadOnlyCollection<BillAggregate> persistedBills)
    {
        ArgumentNullException.ThrowIfNull(persistedBills);

        PersistedBills = persistedBills.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<BillAggregate> PersistedBills { get; }

    public int PersistedCount => PersistedBills.Count;
}
