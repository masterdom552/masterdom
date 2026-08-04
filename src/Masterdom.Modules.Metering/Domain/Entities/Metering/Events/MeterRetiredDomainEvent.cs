using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

public sealed record MeterRetiredDomainEvent(
    MeterId MeterId,
    RemovalDate RemovalDate,
    DateTime OccurredOnUtc) : IDomainEvent;
