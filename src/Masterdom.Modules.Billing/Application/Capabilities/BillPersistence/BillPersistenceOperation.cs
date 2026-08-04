using Masterdom.Modules.Billing.Application.Capabilities.BillPersistence.Contracts;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Modules.Billing.Domain.Repositories;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Capabilities.BillPersistence;

public class BillPersistenceOperation
{
    private readonly IBillRepository _repository;
    private readonly IBillingUnitOfWork _unitOfWork;

    public BillPersistenceOperation(
        IBillRepository repository,
        IBillingUnitOfWork unitOfWork)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public virtual IReadOnlyCollection<BillAggregate> Execute(BillPersistenceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Bills.Count == 0)
        {
            return Array.Empty<BillAggregate>();
        }

        _unitOfWork.Execute(() =>
        {
            foreach (var bill in request.Bills)
            {
                _repository.Add(bill);
            }
        });

        return request.Bills;
    }
}
