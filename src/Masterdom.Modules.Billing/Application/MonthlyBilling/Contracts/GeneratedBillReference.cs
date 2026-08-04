using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling.Contracts;

public sealed record GeneratedBillReference(
    BillId BillId,
    BillNumber BillNumber,
    TenancyReference TenancyReference,
    LeaseReference LeaseReference,
    PropertyReference PropertyReference,
    Guid UnitId,
    decimal TotalAmount,
    decimal OutstandingAmount);
