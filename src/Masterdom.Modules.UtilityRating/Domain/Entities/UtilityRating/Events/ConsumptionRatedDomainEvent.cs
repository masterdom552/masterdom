using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;

public sealed record ConsumptionRatedDomainEvent(
    UtilityRatingId UtilityRatingId,
    Guid MeterId,
    Guid ConsumptionReadingId,
    decimal RatedUnits,
    decimal RatedAmount,
    DateTime OccurredOnUtc) : IDomainEvent;
