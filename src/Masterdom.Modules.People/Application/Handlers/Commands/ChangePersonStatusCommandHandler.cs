using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Commands;

/// <summary>
/// Handles person-status command orchestration.
/// </summary>
public sealed class ChangePersonStatusCommandHandler : ICommandHandler<ChangePersonStatusCommand, ExecutionResult<Person>>
{
    private readonly IPersonApplicationService _applicationService;

    public ChangePersonStatusCommandHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Person> Handle(ChangePersonStatusCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var person = _applicationService.ChangeStatus(command);
            return ExecutionResult<Person>.Success(person);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Person>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
