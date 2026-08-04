using Masterdom.Modules.Tenancy.Application.Queries;
using Masterdom.Modules.Tenancy.Application.Services;
using Masterdom.Modules.Tenancy.Application.Support;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Handlers.Queries;

public sealed class GetTenancyByIdQueryHandler : IQueryHandler<GetTenancyByIdQuery, ExecutionResult<TenancyAggregate>>
{
    private readonly ITenancyApplicationService _applicationService;

    public GetTenancyByIdQueryHandler(ITenancyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<TenancyAggregate> Handle(GetTenancyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var tenancy = _applicationService.GetTenancy(query);
        return tenancy is null
            ? ExecutionResult<TenancyAggregate>.Failure("not_found", $"Tenancy '{query.TenancyId}' was not found.")
            : ExecutionResult<TenancyAggregate>.Success(tenancy);
    }
}
