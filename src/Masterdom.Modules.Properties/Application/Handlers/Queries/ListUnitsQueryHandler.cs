using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Queries;

/// <summary>
/// Handles unit listing query.
/// </summary>
public sealed class ListUnitsQueryHandler
    : IQueryHandler<ListUnitsQuery, ExecutionResult<IReadOnlyCollection<Unit>>>
{
    private readonly IPropertyApplicationService _applicationService;

    public ListUnitsQueryHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<IReadOnlyCollection<Unit>> Handle(ListUnitsQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var units = _applicationService.ListUnits(query);
        return ExecutionResult<IReadOnlyCollection<Unit>>.Success(units);
    }
}
