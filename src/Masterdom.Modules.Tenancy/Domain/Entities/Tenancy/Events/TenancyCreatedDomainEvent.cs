using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record TenancyCreatedDomainEvent(
    TenancyId TenancyId,
    TenancyNumber TenancyNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
