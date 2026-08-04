using Masterdom.Modules.Lease.Domain.Entities.Lease;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Modules.Lease.Domain.Repositories;

public interface ILeaseRepository
{
    void Add(LeaseAggregate lease);

    LeaseAggregate? GetById(LeaseId id);

    LeaseAggregate? GetByNumber(LeaseNumber number);

    bool HasActiveLeaseForTenancy(TenancyReference tenancy);

    void Update(LeaseAggregate lease);
}
