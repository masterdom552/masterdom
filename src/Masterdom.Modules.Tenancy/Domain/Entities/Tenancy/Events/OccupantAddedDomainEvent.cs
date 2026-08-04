using Masterdom.Core.Common.Events;
using Masterdom.Core.Identifiers;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record OccupantAddedDomainEvent(
    TenancyId TenancyId,
    PersonId PersonId,
    bool IsPrimary,
    DateTime OccurredOnUtc) : IDomainEvent;
