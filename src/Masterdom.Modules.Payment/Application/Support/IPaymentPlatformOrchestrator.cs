using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Modules.Payment.Application.Support;

public interface IPaymentPlatformOrchestrator
{
    void OnPaymentMutated(PaymentAggregate payment, string operationName);
}
