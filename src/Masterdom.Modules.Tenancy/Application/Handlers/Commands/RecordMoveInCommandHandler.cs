using Masterdom.Modules.Tenancy.Application.Commands;
using Masterdom.Modules.Tenancy.Application.Services;
using Masterdom.Modules.Tenancy.Application.Support;
using TenancyAggregate = Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Tenancy;

namespace Masterdom.Modules.Tenancy.Application.Handlers.Commands;

public sealed class RecordMoveInCommandHandler : ICommandHandler<RecordMoveInCommand, ExecutionResult<TenancyAggregate>>
{
    private readonly ITenancyApplicationService _applicationService;

    public RecordMoveInCommandHandler(ITenancyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<TenancyAggregate> Handle(RecordMoveInCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var tenancy = _applicationService.RecordMoveIn(command);
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
