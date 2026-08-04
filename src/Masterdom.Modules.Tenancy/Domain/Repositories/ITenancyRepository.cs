using Masterdom.Modules.Tenancy.Domain.Entities.Tenancy;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Domain.Repositories;

public interface ITenancyRepository
{
    void Add(TenancyAggregate tenancy);

    TenancyAggregate? GetById(TenancyId id);

    bool HasActiveTenancyForUnit(UnitReference unit);

    void Update(TenancyAggregate tenancy);
}
