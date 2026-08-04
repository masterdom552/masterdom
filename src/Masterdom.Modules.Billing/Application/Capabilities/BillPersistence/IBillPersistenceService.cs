using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;

namespace Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;

public interface IBillPersistenceService
{
    BillPersistenceResult Persist(BillPersistenceRequest request);
}
