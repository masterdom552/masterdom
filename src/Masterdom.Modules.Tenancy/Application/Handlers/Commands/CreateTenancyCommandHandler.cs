using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Services;
using Masterdom.Modules.Tenancy.Application.Support;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Handlers.Commands;

public sealed class CreateTenancyCommandHandler : ICommandHandler<CreateTenancyCommand, ExecutionResult<TenancyAggregate>>
{
    private readonly ITenancyApplicationService _applicationService;

    public CreateTenancyCommandHandler(ITenancyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<TenancyAggregate> Handle(CreateTenancyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var tenancy = _applicationService.CreateTenancy(command);
            return ExecutionResult<TenancyAggregate>.Success(tenancy);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<TenancyAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<TenancyAggregate>.Failure("conflict", ex.Message);
        }
    }
}
