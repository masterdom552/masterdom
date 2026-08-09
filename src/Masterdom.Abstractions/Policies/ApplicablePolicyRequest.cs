namespace Masterdom.Abstractions.Policies;

/// <summary>
/// Describes a consumer request for the policy applicable to a scope and date.
/// </summary>
/// <param name="Consumer">The business capability requesting policy resolution.</param>
/// <param name="PolicyCode">The stable code of the requested policy.</param>
/// <param name="PolicyType">The type of policy to resolve.</param>
/// <param name="ScopeKind">The kind of scope to which the policy applies.</param>
/// <param name="ScopeKey">The identifier of the applicable scope.</param>
/// <param name="AsOfDate">The date for which applicability is evaluated.</param>
public sealed record ApplicablePolicyRequest(
    string Consumer,
    string PolicyCode,
    string PolicyType,
    string ScopeKind,
    string ScopeKey,
    DateOnly AsOfDate);
