using Masterdom.Modules.Billing.Domain.Entities.Billing;

namespace Masterdom.Modules.Billing.Application.Queries;

public sealed record GetBillByIdQuery(BillId BillId);
