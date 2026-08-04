using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Commands;

public sealed record ApplyCreditCommand(
    BillId BillId,
    CreditLine Credit,
    GeneratedDate GeneratedDate,
    IssueDate IssueDate,
    DueDate DueDate);
