using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Commands;

/// <summary>
/// Handles person-creation command orchestration.
/// </summary>
public sealed class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, ExecutionResult<Person>>
{
    private readonly IPersonApplicationService _applicationService;

    public CreatePersonCommandHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Person> Handle(CreatePersonCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var person = _applicationService.CreatePerson(command);
            return ExecutionResult<Person>.Success(person);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<Person>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Person>.Failure("conflict", ex.Message);
        }
    }
}
