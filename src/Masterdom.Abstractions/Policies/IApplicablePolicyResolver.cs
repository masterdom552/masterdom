namespace Masterdom.Abstractions.Policies;

/// <summary>
/// Resolves the policy applicable to a consumer request.
/// </summary>
public interface IApplicablePolicyResolver
{
    /// <summary>
    /// Resolves the policy applicable to the supplied request.
    /// </summary>
    /// <param name="request">The policy applicability request.</param>
    /// <returns>The applicability resolution.</returns>
    ApplicablePolicyResolution Resolve(ApplicablePolicyRequest request);
}
