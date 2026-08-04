using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

public sealed record ReadingCorrectedDomainEvent(
    MeterId MeterId,
    Guid ReadingId,
    DateTime OccurredOnUtc) : IDomainEvent;
