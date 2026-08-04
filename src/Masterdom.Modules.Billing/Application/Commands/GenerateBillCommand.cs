using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Core.Financial.ValueObjects;

namespace Masterdom.Modules.Billing.Application.Commands;

public sealed record GenerateBillCommand(
    BillNumber BillNumber,
    TenancyReference TenancyReference,
    LeaseReference LeaseReference,
    PropertyReference PropertyReference,
    PersonReference BilledParty,
    BillingPeriod BillingPeriod,
    BillingCycle BillingCycle,
    GeneratedDate GeneratedDate,
    IssueDate IssueDate,
    DueDate DueDate,
    Currency Currency,
    ChargeCollection Charges);
