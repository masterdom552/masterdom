using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record MoveInRecordedDomainEvent(
    TenancyId TenancyId,
    MoveInDate MoveInDate,
    DateTime OccurredOnUtc) : IDomainEvent;
