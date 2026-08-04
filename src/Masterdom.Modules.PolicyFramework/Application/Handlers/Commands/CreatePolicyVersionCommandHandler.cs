using Masterdom.Modules.PolicyFramework.Application.Commands;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Application.Support;
using PolicyAggregate = Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Policy;

namespace Masterdom.Modules.PolicyFramework.Application.Handlers.Commands;

public sealed class CreatePolicyVersionCommandHandler : ICommandHandler<CreatePolicyVersionCommand, ExecutionResult<PolicyAggregate>>
{
    private readonly IPolicyFrameworkApplicationService _applicationService;

    public CreatePolicyVersionCommandHandler(IPolicyFrameworkApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<PolicyAggregate> Handle(CreatePolicyVersionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var policy = _applicationService.CreatePolicyVersion(command);
            return ExecutionResult<PolicyAggregate>.Success(policy);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<PolicyAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<PolicyAggregate>.Failure("conflict", ex.Message);
        }
    }
}
