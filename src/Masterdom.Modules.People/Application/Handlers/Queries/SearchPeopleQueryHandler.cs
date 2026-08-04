using Masterdom.Modules.People.Domain.Entities.Person;
using Masterdom.Modules.People.Application.Queries;
using Masterdom.Modules.People.Application.Services;
using Masterdom.Modules.People.Application.Support;

namespace Masterdom.Modules.People.Application.Handlers.Queries;

/// <summary>
/// Handles search-ready people retrieval query.
/// </summary>
public sealed class SearchPeopleQueryHandler : IQueryHandler<SearchPeopleQuery, ExecutionResult<IReadOnlyCollection<Person>>>
{
    private readonly IPersonApplicationService _applicationService;

    public SearchPeopleQueryHandler(IPersonApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<Person>> Handle(SearchPeopleQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var people = _applicationService.SearchPeople(query);
        return ExecutionResult<IReadOnlyCollection<Person>>.Success(people);
    }
}
