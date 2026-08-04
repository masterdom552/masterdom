using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;

public sealed record RatingRecalculatedDomainEvent(
    UtilityRatingId PreviousUtilityRatingId,
    UtilityRatingId NewUtilityRatingId,
    int NewVersion,
    DateTime OccurredOnUtc) : IDomainEvent;
