using Masterdom.Modules.Payment.Contracts.Billing;
using Masterdom.Modules.Payment.Domain.Entities.Payment;

namespace Masterdom.Modules.Payment.Application.Commands;

public sealed record AllocatePaymentCommand(
    PaymentId PaymentId,
    IReadOnlyCollection<BillSettlementContract> BillSettlements,
    DateTime AllocatedAtUtc);

public static class AllocatePaymentCommandFactory
{
    public static AllocatePaymentCommand Create(
        PaymentId paymentId,
        IEnumerable<(Guid BillId, string BillNumber, decimal OutstandingAmount, DateOnly DueDate, decimal AllocationAmount)> billSettlements,
        DateTime allocatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(paymentId);
        ArgumentNullException.ThrowIfNull(billSettlements);

        var contracts = billSettlements
            .Select(x => new BillSettlementContract(x.BillId, x.BillNumber, x.OutstandingAmount, x.DueDate, x.AllocationAmount))
            .ToList();

        return new AllocatePaymentCommand(paymentId, contracts, allocatedAtUtc);
    }
}
