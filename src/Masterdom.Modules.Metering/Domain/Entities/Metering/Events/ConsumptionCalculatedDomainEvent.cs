using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

public sealed record ConsumptionCalculatedDomainEvent(
    MeterId MeterId,
    Guid ReadingId,
    decimal ConsumptionValue,
    DateTime OccurredOnUtc) : IDomainEvent;
