using Masterdom.Modules.Lease.Application.Commands;
using Masterdom.Modules.Lease.Application.Services;
using Masterdom.Modules.Lease.Application.Support;
using LeaseAggregate = Masterdom.Modules.Lease.Domain.Entities.Lease.Lease;

namespace Masterdom.Modules.Lease.Application.Handlers.Commands;

public sealed class ChangeCommercialTermsCommandHandler : ICommandHandler<ChangeCommercialTermsCommand, ExecutionResult<LeaseAggregate>>
{
    private readonly ILeaseApplicationService _applicationService;

    public ChangeCommercialTermsCommandHandler(ILeaseApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<LeaseAggregate> Handle(ChangeCommercialTermsCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var lease = _applicationService.ChangeCommercialTerms(command);
            return ExecutionResult<LeaseAggregate>.Success(lease);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<LeaseAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<LeaseAggregate>.Failure("conflict", ex.Message);
        }
    }
}
