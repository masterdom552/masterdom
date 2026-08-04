using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Commands;

public sealed record AddAdjustmentCommand(
    BillId BillId,
    AdjustmentLine Adjustment,
    GeneratedDate GeneratedDate,
    IssueDate IssueDate,
    DueDate DueDate);
