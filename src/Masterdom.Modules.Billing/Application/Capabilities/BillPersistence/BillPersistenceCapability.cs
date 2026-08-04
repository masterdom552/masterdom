using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;

namespace Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;

public class BillPersistenceCapability
{
    private readonly IBillPersistenceService _billPersistenceService;

    public BillPersistenceCapability(IBillPersistenceService billPersistenceService)
    {
        _billPersistenceService = billPersistenceService ?? throw new ArgumentNullException(nameof(billPersistenceService));
    }

    public virtual BillPersistenceResult Persist(BillPersistenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _billPersistenceService.Persist(request);
    }
}
