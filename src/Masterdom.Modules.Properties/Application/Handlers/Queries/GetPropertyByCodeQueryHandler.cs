using Masterdom.Modules.Properties.Application.Queries;
using Masterdom.Modules.Properties.Application.Services;
using Masterdom.Modules.Properties.Application.Support;
using Masterdom.Modules.Properties.Domain.Entities.Property;

namespace Masterdom.Modules.Properties.Application.Handlers.Queries;

/// <summary>
/// Handles property retrieval by business code.
/// </summary>
public sealed class GetPropertyByCodeQueryHandler
    : IQueryHandler<GetPropertyByCodeQuery, ExecutionResult<Property>>
{
    private readonly IPropertyApplicationService _applicationService;

    public GetPropertyByCodeQueryHandler(IPropertyApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<Property> Handle(GetPropertyByCodeQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var property = _applicationService.GetPropertyByCode(query);
        return property is null
            ? ExecutionResult<Property>.Failure("not_found", $"Property code '{query.Code.Value}' was not found.")
            : ExecutionResult<Property>.Success(property);
    }
}
