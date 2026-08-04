using Masterdom.Core.Common.Events;
using Masterdom.Core.Identifiers;

namespace Masterdom.Modules.Tenancy.Domain.Entities.Tenancy.Events;

public sealed record OccupantRemovedDomainEvent(
    TenancyId TenancyId,
    PersonId PersonId,
    DateTime OccurredOnUtc) : IDomainEvent;
