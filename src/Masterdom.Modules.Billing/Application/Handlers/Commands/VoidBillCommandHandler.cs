using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Services;
using Masterdom.Modules.Billing.Application.Support;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Handlers.Commands;

public sealed class VoidBillCommandHandler : ICommandHandler<VoidBillCommand, ExecutionResult<BillAggregate>>
{
    private readonly IBillingApplicationService _applicationService;

    public VoidBillCommandHandler(IBillingApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<BillAggregate> Handle(VoidBillCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var bill = _applicationService.VoidBill(command);
            return ExecutionResult<BillAggregate>.Success(bill);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<BillAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<BillAggregate>.Failure("conflict", ex.Message);
        }
    }
}
