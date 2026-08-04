using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Events;

public sealed record PolicyActivatedDomainEvent(
    PolicyId PolicyId,
    int VersionNumber,
    DateTime OccurredOnUtc) : IDomainEvent;
