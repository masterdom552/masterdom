using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Services;
using Masterdom.Modules.Payment.Application.Support;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Modules.Payment.Application.Handlers.Commands;

public sealed class ReceivePaymentCommandHandler : ICommandHandler<ReceivePaymentCommand, ExecutionResult<PaymentAggregate>>
{
    private readonly IPaymentApplicationService _applicationService;

    public ReceivePaymentCommandHandler(IPaymentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<PaymentAggregate> Handle(ReceivePaymentCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var payment = _applicationService.ReceivePayment(command);
            return ExecutionResult<PaymentAggregate>.Success(payment);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<PaymentAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<PaymentAggregate>.Failure("conflict", ex.Message);
        }
    }
}
