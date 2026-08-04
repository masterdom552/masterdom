using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Events;

public sealed record PolicyArchivedDomainEvent(
    PolicyId PolicyId,
    string Reason,
    DateTime OccurredOnUtc) : IDomainEvent;
