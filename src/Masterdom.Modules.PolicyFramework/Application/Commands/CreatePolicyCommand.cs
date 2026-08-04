using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Commands;

public sealed record CreatePolicyCommand(
    PolicyType PolicyType,
    PolicyCategory PolicyCategory,
    PolicyReference PolicyReference,
    PolicyScope Scope,
    PolicyCondition Condition,
    PolicyMetadata Metadata,
    EffectiveDateRange EffectiveDateRange,
    DateTime CreatedAtUtc);
