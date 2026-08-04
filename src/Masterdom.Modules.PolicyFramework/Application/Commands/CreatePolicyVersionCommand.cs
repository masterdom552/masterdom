using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Commands;

public sealed record CreatePolicyVersionCommand(
    PolicyId PolicyId,
    PolicyCondition Condition,
    PolicyMetadata Metadata,
    EffectiveDateRange EffectiveDateRange,
    DateTime CreatedAtUtc);
