using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Queries;

/// <summary>
/// Handles search-ready property retrieval query.
/// </summary>
public sealed class SearchPropertiesQueryHandler
    : IQueryHandler<SearchPropertiesQuery, ExecutionResult<IReadOnlyCollection<Property>>>
{
    private readonly IPropertyApplicationService _applicationService;

    public SearchPropertiesQueryHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<Property>> Handle(SearchPropertiesQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var properties = _applicationService.SearchProperties(query);
        return ExecutionResult<IReadOnlyCollection<Property>>.Success(properties);
    }
}
