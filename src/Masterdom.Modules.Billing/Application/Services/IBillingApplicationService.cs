using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Queries;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.Services;

public interface IBillingApplicationService
{
    BillAggregate GenerateBill(GenerateBillCommand command);

    BillAggregate FinalizeBill(FinalizeBillCommand command);

    BillAggregate AddAdjustment(AddAdjustmentCommand command);

    BillAggregate ApplyCredit(ApplyCreditCommand command);

    BillAggregate VoidBill(VoidBillCommand command);

    BillAggregate? GetBill(GetBillByIdQuery query);

    BillAggregate? GetBillByNumber(GetBillByNumberQuery query);
}
