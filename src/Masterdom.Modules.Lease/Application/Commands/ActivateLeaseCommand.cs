using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Modules.Lease.Application.Commands;

public sealed record ActivateLeaseCommand(LeaseId LeaseId);
