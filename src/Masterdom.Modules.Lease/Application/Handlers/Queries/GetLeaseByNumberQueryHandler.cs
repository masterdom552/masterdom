using Masterdom.Modules.Lease.Application.Queries;
using Masterdom.Modules.Lease.Application.Services;
using Masterdom.Modules.Lease.Application.Support;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Modules.Lease.Application.Handlers.Queries;

public sealed class GetLeaseByNumberQueryHandler : IQueryHandler<GetLeaseByNumberQuery, ExecutionResult<LeaseAggregate>>
{
    private readonly ILeaseApplicationService _applicationService;

    public GetLeaseByNumberQueryHandler(ILeaseApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<LeaseAggregate> Handle(GetLeaseByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var lease = _applicationService.GetLeaseByNumber(query);
        return lease is null
            ? ExecutionResult<LeaseAggregate>.Failure("not_found", $"Lease '{query.Number.Value}' was not found.")
            : ExecutionResult<LeaseAggregate>.Success(lease);
    }
}
