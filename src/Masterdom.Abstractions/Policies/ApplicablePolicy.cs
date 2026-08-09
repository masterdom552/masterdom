namespace Masterdom.Abstractions.Policies;

/// <summary>
/// Represents a policy and version selected by applicability resolution.
/// </summary>
/// <param name="PolicyId">The policy identifier.</param>
/// <param name="PolicyCode">The stable policy code.</param>
/// <param name="DisplayName">The policy display name.</param>
/// <param name="PolicyType">The policy type.</param>
/// <param name="PolicyCategory">The policy category.</param>
/// <param name="ScopeKind">The kind of scope to which the policy applies.</param>
/// <param name="ScopeKey">The identifier of the applicable scope.</param>
/// <param name="VersionNumber">The selected policy version number.</param>
/// <param name="EffectiveFrom">The first date on which the version is effective.</param>
/// <param name="EffectiveTo">The optional last date on which the version is effective.</param>
/// <param name="SelectorKey">The key identifying the policy selector.</param>
/// <param name="SelectorDefinition">The policy selector definition.</param>
/// <param name="Metadata">The metadata associated with the selected version.</param>
public sealed record ApplicablePolicy(
    Guid PolicyId,
    string PolicyCode,
    string DisplayName,
    string PolicyType,
    string PolicyCategory,
    string ScopeKind,
    string ScopeKey,
    int VersionNumber,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string SelectorKey,
    string SelectorDefinition,
    IReadOnlyDictionary<string, string> Metadata);
