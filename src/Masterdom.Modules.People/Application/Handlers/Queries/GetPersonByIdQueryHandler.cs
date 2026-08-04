using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Queries;

/// <summary>
/// Handles person retrieval by identifier.
/// </summary>
public sealed class GetPersonByIdQueryHandler : IQueryHandler<GetPersonByIdQuery, ExecutionResult<Person>>
{
    private readonly IPersonApplicationService _applicationService;

    public GetPersonByIdQueryHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Person> Handle(GetPersonByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var person = _applicationService.GetPerson(query);
        return person is null
            ? ExecutionResult<Person>.Failure("not_found", $"Person '{query.PersonId}' was not found.")
            : ExecutionResult<Person>.Success(person);
    }
}
