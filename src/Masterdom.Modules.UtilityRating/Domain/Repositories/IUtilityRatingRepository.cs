using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;
using UtilityRatingAggregate = Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Domain.Repositories;

public interface IUtilityRatingRepository
{
    void Add(UtilityRatingAggregate rating);

    void Update(UtilityRatingAggregate rating);

    UtilityRatingAggregate? GetById(UtilityRatingId id);

    UtilityRatingAggregate? GetByMeterPeriodAndVersion(
        MeterReference meterReference,
        RatingPeriod ratingPeriod,
        RatingVersion ratingVersion);

    UtilityRatingAggregate? GetLatestByMeterAndPeriod(
        MeterReference meterReference,
        RatingPeriod ratingPeriod);
}
