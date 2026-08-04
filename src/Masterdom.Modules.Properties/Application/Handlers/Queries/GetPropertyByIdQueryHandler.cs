using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Queries;

/// <summary>
/// Handles property retrieval by identifier.
/// </summary>
public sealed class GetPropertyByIdQueryHandler
    : IQueryHandler<GetPropertyByIdQuery, ExecutionResult<Property>>
{
    private readonly IPropertyApplicationService _applicationService;

    public GetPropertyByIdQueryHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Property> Handle(GetPropertyByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var property = _applicationService.GetProperty(query);
        return property is null
            ? ExecutionResult<Property>.Failure("not_found", $"Property '{query.PropertyId}' was not found.")
            : ExecutionResult<Property>.Success(property);
    }
}
