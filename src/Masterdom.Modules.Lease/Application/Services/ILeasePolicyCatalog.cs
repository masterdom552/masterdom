using Masterdom.Abstractions.Policies;

namespace Masterdom.Modules.Lease.Application.Services;

/// <summary>
/// Provides Lease application services with applicable policy references.
/// </summary>
public interface ILeasePolicyCatalog
{
    /// <summary>
    /// Resolves the renewal policy applicable to a Lease scope and date.
    /// </summary>
    /// <param name="policyReference">The renewal policy reference.</param>
    /// <param name="scopeKey">The Lease policy scope identifier.</param>
    /// <param name="asOfDate">The date for which applicability is evaluated.</param>
    /// <returns>The applicable-policy resolution.</returns>
    ApplicablePolicyResolution ResolveRenewalPolicy(
        string policyReference,
        string scopeKey,
        DateOnly asOfDate);
}
