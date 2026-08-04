using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Billing.Application.Services;
using Masterdom.Modules.Billing.Application.Support;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Handlers.Queries;

public sealed class GetBillByNumberQueryHandler : IQueryHandler<GetBillByNumberQuery, ExecutionResult<BillAggregate>>
{
    private readonly IBillingApplicationService _applicationService;

    public GetBillByNumberQueryHandler(IBillingApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<BillAggregate> Handle(GetBillByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var bill = _applicationService.GetBillByNumber(query);
        return bill is null
            ? ExecutionResult<BillAggregate>.Failure("not_found", $"Bill '{query.BillNumber}' was not found.")
            : ExecutionResult<BillAggregate>.Success(bill);
    }
}
