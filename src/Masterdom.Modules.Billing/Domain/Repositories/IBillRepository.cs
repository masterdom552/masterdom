using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Domain.Repositories;

public interface IBillRepository
{
    void Add(BillAggregate bill);

    BillAggregate? GetById(BillId id);

    BillAggregate? GetByNumber(BillNumber number);

    void Update(BillAggregate bill);
}
