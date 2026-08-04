using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Modules.Lease.Application.Queries;

public sealed record GetLeaseByNumberQuery(LeaseNumber Number);
