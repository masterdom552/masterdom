using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record MoveOutRecordedDomainEvent(
    TenancyId TenancyId,
    MoveOutDate MoveOutDate,
    DateTime OccurredOnUtc) : IDomainEvent;
