using Masterdom.Modules.Payment.Domain.Entities.Payment;

namespace Masterdom.Modules.Payment.Application.Commands;

public sealed record VoidPaymentCommand(
    PaymentId PaymentId,
    string Reason,
    DateTime VoidedAtUtc);
