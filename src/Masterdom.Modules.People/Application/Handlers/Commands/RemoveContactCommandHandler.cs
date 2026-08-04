using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Commands;

/// <summary>
/// Handles remove-contact command orchestration.
/// </summary>
public sealed class RemoveContactCommandHandler : ICommandHandler<RemoveContactCommand, ExecutionResult<bool>>
{
    private readonly IPersonApplicationService _applicationService;

    public RemoveContactCommandHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<bool> Handle(RemoveContactCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var removed = _applicationService.RemoveContact(command);
        return removed
            ? ExecutionResult<bool>.Success(true)
            : ExecutionResult<bool>.Failure("not_found", "The requested contact was not found.");
    }
}
