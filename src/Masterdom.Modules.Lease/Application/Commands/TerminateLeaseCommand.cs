using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Modules.Lease.Application.Commands;

public sealed record TerminateLeaseCommand(
    LeaseId LeaseId,
    TerminationReason Reason);
