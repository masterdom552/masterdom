using Masterdom.Modules.UtilityRating.Application.Queries;
using Masterdom.Modules.UtilityRating.Application.Services;
using Masterdom.Modules.UtilityRating.Application.Support;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Handlers.Queries;

public sealed class GetRatingByIdQueryHandler : IQueryHandler<GetRatingByIdQuery, ExecutionResult<UtilityRatingAggregate>>
{
    private readonly IUtilityRatingApplicationService _applicationService;

    public GetRatingByIdQueryHandler(IUtilityRatingApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<UtilityRatingAggregate> Handle(GetRatingByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rating = _applicationService.GetRating(query);

        return rating is null
            ? ExecutionResult<UtilityRatingAggregate>.Failure("not_found", "Utility rating was not found.")
            : ExecutionResult<UtilityRatingAggregate>.Success(rating);
    }
}
