using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record TenancyArchivedDomainEvent(
    TenancyId TenancyId,
    DateTime OccurredOnUtc) : IDomainEvent;
