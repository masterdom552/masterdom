using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Queries;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Modules.Lease.Application.Services;

public interface ILeaseApplicationService
{
    LeaseAggregate CreateLease(CreateLeaseCommand command);

    LeaseAggregate ActivateLease(ActivateLeaseCommand command);

    LeaseAggregate RenewLease(RenewLeaseCommand command);

    LeaseAggregate TerminateLease(TerminateLeaseCommand command);

    LeaseAggregate ExpireLease(ExpireLeaseCommand command);

    LeaseAggregate ChangeCommercialTerms(ChangeCommercialTermsCommand command);

    LeaseAggregate CloseLease(CloseLeaseCommand command);

    LeaseAggregate? GetLease(GetLeaseByIdQuery query);

    LeaseAggregate? GetLeaseByNumber(GetLeaseByNumberQuery query);
}
