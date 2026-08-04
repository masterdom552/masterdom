using Masterdom.Modules.Payment.Domain.Entities.Payment;

namespace Masterdom.Modules.Payment.Application.Queries;

public sealed record GetPaymentByReferenceQuery(PaymentReference PaymentReference);
