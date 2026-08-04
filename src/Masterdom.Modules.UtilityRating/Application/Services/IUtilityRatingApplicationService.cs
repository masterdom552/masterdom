using Masterdom.Modules.UtilityRating.Application.Commands;
using Masterdom.Modules.UtilityRating.Application.Queries;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Services;

public interface IUtilityRatingApplicationService
{
    UtilityRatingAggregate RateConsumption(RateConsumptionCommand command);

    UtilityRatingAggregate RecalculateRating(RecalculateRatingCommand command);

    UtilityRatingAggregate ApproveRating(ApproveRatingCommand command);

    UtilityRatingAggregate ArchiveRating(ArchiveRatingCommand command);

    UtilityRatingAggregate? GetRating(GetRatingByIdQuery query);

    UtilityRatingAggregate? GetLatestRating(GetLatestRatingQuery query);
}
