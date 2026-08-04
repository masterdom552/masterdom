using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Commands;

/// <summary>
/// Handles person-rename command orchestration.
/// </summary>
public sealed class RenamePersonCommandHandler : ICommandHandler<RenamePersonCommand, ExecutionResult<Person>>
{
    private readonly IPersonApplicationService _applicationService;

    public RenamePersonCommandHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Person> Handle(RenamePersonCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var person = _applicationService.RenamePerson(command);
            return ExecutionResult<Person>.Success(person);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Person>.Failure("not_found", ex.Message);
        }
    }
}
