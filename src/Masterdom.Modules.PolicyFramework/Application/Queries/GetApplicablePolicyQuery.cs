using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Queries;

public sealed record GetApplicablePolicyQuery(
    PolicyType PolicyType,
    PolicyScope Scope,
    DateOnly AsOfDate);
