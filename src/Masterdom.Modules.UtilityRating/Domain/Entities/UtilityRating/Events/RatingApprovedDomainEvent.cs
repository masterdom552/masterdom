using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;

public sealed record RatingApprovedDomainEvent(
    UtilityRatingId UtilityRatingId,
    int RatingVersion,
    DateTime OccurredOnUtc) : IDomainEvent;
