using Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating;

namespace Masterdom.Modules.UtilityRating.Application.Queries;

public sealed record GetLatestRatingQuery(
    MeterReference MeterReference,
    RatingPeriod RatingPeriod);
