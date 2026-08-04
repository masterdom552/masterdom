using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Commands;

public sealed record VoidBillCommand(BillId BillId, string Reason);
