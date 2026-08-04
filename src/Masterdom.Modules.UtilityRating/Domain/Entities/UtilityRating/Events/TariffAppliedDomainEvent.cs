using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.UtilityRating.Domain.Entities.UtilityRating.Events;

public sealed record TariffAppliedDomainEvent(
    UtilityRatingId UtilityRatingId,
    string TariffCode,
    int TariffVersion,
    DateTime OccurredOnUtc) : IDomainEvent;
