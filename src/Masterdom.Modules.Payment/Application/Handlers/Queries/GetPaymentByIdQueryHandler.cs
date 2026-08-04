using Masterdom.Modules.Payment.Application.Queries;
using Masterdom.Modules.Payment.Application.Services;
using Masterdom.Modules.Payment.Application.Support;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Modules.Payment.Application.Handlers.Queries;

public sealed class GetPaymentByIdQueryHandler : IQueryHandler<GetPaymentByIdQuery, ExecutionResult<PaymentAggregate>>
{
    private readonly IPaymentApplicationService _applicationService;

    public GetPaymentByIdQueryHandler(IPaymentApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<PaymentAggregate> Handle(GetPaymentByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var payment = _applicationService.GetPayment(query);
        return payment is null
            ? ExecutionResult<PaymentAggregate>.Failure("not_found", $"Payment '{query.PaymentId}' was not found.")
            : ExecutionResult<PaymentAggregate>.Success(payment);
    }
}
