using Masterdom.Modules.Payment.Domain.Entities.Payment;

namespace Masterdom.Modules.Payment.Application.Commands;

public sealed record ReceivePaymentCommand(
    PaymentReference PaymentReference,
    PaymentAmount PaymentAmount,
    PaymentDate PaymentDate,
    PaymentMethod PaymentMethod,
    PaymentChannel PaymentChannel,
    PaymentSource PaymentSource,
    DateTime ReceivedAtUtc);
