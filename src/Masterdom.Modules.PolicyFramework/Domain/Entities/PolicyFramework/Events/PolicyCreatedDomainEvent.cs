using Masterdom.Core.Common.Events;

namespace Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework.Events;

public sealed record PolicyCreatedDomainEvent(
    PolicyId PolicyId,
    string PolicyType,
    string PolicyCategory,
    string ScopeKind,
    string ScopeKey,
    DateTime OccurredOnUtc) : IDomainEvent;
