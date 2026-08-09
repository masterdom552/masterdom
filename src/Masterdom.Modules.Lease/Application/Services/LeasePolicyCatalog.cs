using Masterdom.Abstractions.Policies;

namespace Masterdom.Modules.Lease.Application.Services;

/// <summary>
/// Translates Lease renewal policy references into shared applicability requests.
/// </summary>
public sealed class LeasePolicyCatalog : ILeasePolicyCatalog
{
    private const string Consumer = "lease";
    private const string RenewalPolicyType = "renewal";
    private const string ModuleScopeKind = "Module";

    private readonly IApplicablePolicyResolver _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeasePolicyCatalog"/> class.
    /// </summary>
    /// <param name="resolver">The shared applicable-policy resolver.</param>
    public LeasePolicyCatalog(IApplicablePolicyResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <inheritdoc />
    public ApplicablePolicyResolution ResolveRenewalPolicy(
        string policyReference,
        string scopeKey,
        DateOnly asOfDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyReference);
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeKey);

        return _resolver.Resolve(new ApplicablePolicyRequest(
            Consumer,
            policyReference.Trim(),
            RenewalPolicyType,
            ModuleScopeKind,
            scopeKey.Trim(),
            asOfDate));
    }
}
