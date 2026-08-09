using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Modules.PolicyFramework.Application.Queries;

public sealed record GetApplicablePolicyQuery(
    PolicyType PolicyType,
    PolicyScope Scope,
    DateOnly AsOfDate,
    string? PolicyCode = null)
{
    /// <summary>
    /// Gets the optional stable policy code used to disambiguate applicability resolution.
    /// </summary>
    public string? PolicyCode { get; init; } = PolicyCode;
}
