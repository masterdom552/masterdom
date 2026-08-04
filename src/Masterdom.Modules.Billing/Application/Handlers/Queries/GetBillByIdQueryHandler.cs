using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Billing.Application.Services;
using Masterdom.Modules.Billing.Application.Support;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Handlers.Queries;

public sealed class GetBillByIdQueryHandler : IQueryHandler<GetBillByIdQuery, ExecutionResult<BillAggregate>>
{
    private readonly IBillingApplicationService _applicationService;

    public GetBillByIdQueryHandler(IBillingApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<BillAggregate> Handle(GetBillByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var bill = _applicationService.GetBill(query);
        return bill is null
            ? ExecutionResult<BillAggregate>.Failure("not_found", $"Bill '{query.BillId}' was not found.")
            : ExecutionResult<BillAggregate>.Success(bill);
    }
}
