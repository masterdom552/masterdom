using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;

public sealed record RatingArchivedDomainEvent(
    UtilityRatingId UtilityRatingId,
    int RatingVersion,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
