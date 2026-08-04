using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Queries;

/// <summary>
/// Handles person retrieval by business number.
/// </summary>
public sealed class GetPersonByNumberQueryHandler : IQueryHandler<GetPersonByNumberQuery, ExecutionResult<Person>>
{
    private readonly IPersonApplicationService _applicationService;

    public GetPersonByNumberQueryHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Person> Handle(GetPersonByNumberQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var person = _applicationService.GetPersonByNumber(query);
        return person is null
            ? ExecutionResult<Person>.Failure("not_found", $"Person number '{query.Number.Value}' was not found.")
            : ExecutionResult<Person>.Success(person);
    }
}
