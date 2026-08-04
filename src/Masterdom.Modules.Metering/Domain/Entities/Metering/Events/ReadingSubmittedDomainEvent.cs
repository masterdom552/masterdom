using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

public sealed record ReadingSubmittedDomainEvent(
    MeterId MeterId,
    Guid ReadingId,
    ReadingDate ReadingDate,
    DateTime OccurredOnUtc) : IDomainEvent;
