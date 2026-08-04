using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;

public sealed class BillPersistenceRequest
{
    public BillPersistenceRequest(IReadOnlyCollection<BillAggregate> bills)
    {
        ArgumentNullException.ThrowIfNull(bills);

        Bills = bills.ToList().AsReadOnly();
    }

    public IReadOnlyCollection<BillAggregate> Bills { get; }
}
