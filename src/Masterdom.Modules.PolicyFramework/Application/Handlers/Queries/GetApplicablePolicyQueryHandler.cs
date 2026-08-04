using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Application.Support;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Modules.PolicyFramework.Application.Handlers.Queries;

public sealed class GetApplicablePolicyQueryHandler : IQueryHandler<GetApplicablePolicyQuery, ExecutionResult<PolicyAggregate>>
{
    private readonly IPolicyFrameworkApplicationService _applicationService;

    public GetApplicablePolicyQueryHandler(IPolicyFrameworkApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<PolicyAggregate> Handle(GetApplicablePolicyQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var policy = _applicationService.GetApplicablePolicy(query);
        return policy is null
            ? ExecutionResult<PolicyAggregate>.Failure("not_found", "No applicable policy was found.")
            : ExecutionResult<PolicyAggregate>.Success(policy);
    }
}
