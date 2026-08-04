using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Metering.Domain.Entities.Metering.Events;

public sealed record MeterInstalledDomainEvent(
    MeterId MeterId,
    MeterNumber MeterNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
