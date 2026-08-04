using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Commands;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Commands;

/// <summary>
/// Handles add-contact command orchestration.
/// </summary>
public sealed class AddContactCommandHandler : ICommandHandler<AddContactCommand, ExecutionResult<Person>>
{
    private readonly IPersonApplicationService _applicationService;

    public AddContactCommandHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Person> Handle(AddContactCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var person = _applicationService.AddContact(command);
            return ExecutionResult<Person>.Success(person);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<Person>.Failure("domain_rule_violation", ex.Message);
        }
    }
}
