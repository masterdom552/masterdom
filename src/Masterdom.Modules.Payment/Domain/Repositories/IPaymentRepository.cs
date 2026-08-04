using Masterdom.Modules.Payment.Domain.Entities.Payment;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Modules.Payment.Domain.Repositories;

public interface IPaymentRepository
{
    void Add(PaymentAggregate payment);

    void Update(PaymentAggregate payment);

    PaymentAggregate? GetById(PaymentId id);

    PaymentAggregate? GetByReference(PaymentReference paymentReference);
}
