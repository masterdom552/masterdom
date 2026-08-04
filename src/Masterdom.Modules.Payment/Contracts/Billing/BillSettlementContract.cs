namespace Masterdom.Modules.Payment.Contracts.Billing;

public sealed record BillSettlementContract(
    Guid BillId,
    string BillNumber,
    decimal OutstandingAmount,
    DateOnly DueDate,
    decimal AllocationAmount);
