using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Modules.Billing.Application.MonthlyBilling;

/// <summary>
/// Construction helper that creates a Bill aggregate from an already-composed command.
/// </summary>
public class MonthlyBillFactory
{
    public virtual BillAggregate Generate(GenerateBillCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return BillAggregate.Generate(
            BillId.New(),
            command.BillNumber,
            command.TenancyReference,
            command.LeaseReference,
            command.PropertyReference,
            command.BilledParty,
            command.BillingPeriod,
            command.BillingCycle,
            command.GeneratedDate,
            command.IssueDate,
            command.DueDate,
            command.Currency,
            command.Charges);
    }
}
