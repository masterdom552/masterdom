using Masterdom.Modules.Payment.Domain.Entities.Payment;

namespace Masterdom.Modules.Payment.Application.Commands;

public sealed record ReversePaymentCommand(
    PaymentId PaymentId,
    string Reason,
    DateTime ReversedAtUtc);
