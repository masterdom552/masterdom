using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Queries;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Modules.Payment.Application.Services;

public interface IPaymentApplicationService
{
    PaymentAggregate ReceivePayment(ReceivePaymentCommand command);

    PaymentAggregate AllocatePayment(AllocatePaymentCommand command);

    PaymentAggregate ReversePayment(ReversePaymentCommand command);

    PaymentAggregate VoidPayment(VoidPaymentCommand command);

    PaymentAggregate? GetPayment(GetPaymentByIdQuery query);

    PaymentAggregate? GetPayment(GetPaymentByReferenceQuery query);
}
