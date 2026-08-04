using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Application.Support;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Modules.PolicyFramework.Application.Handlers.Queries;

public sealed class GetPolicyByIdQueryHandler : IQueryHandler<GetPolicyByIdQuery, ExecutionResult<PolicyAggregate>>
{
    private readonly IPolicyFrameworkApplicationService _applicationService;

    public GetPolicyByIdQueryHandler(IPolicyFrameworkApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<PolicyAggregate> Handle(GetPolicyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var policy = _applicationService.GetPolicy(query);
        return policy is null
            ? ExecutionResult<PolicyAggregate>.Failure("not_found", $"Policy '{query.PolicyId}' was not found.")
            : ExecutionResult<PolicyAggregate>.Success(policy);
    }
}
