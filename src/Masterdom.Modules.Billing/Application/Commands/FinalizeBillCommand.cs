using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Commands;

public sealed record FinalizeBillCommand(BillId BillId);
