using Masterdom.Abstractions.Policies;
using Masterdom.Modules.Lease.Application.Services;
using Masterdom.Modules.Lease.Domain.Entities.Lease;

namespace Masterdom.Core.Tests.Lease;

public sealed class LeasePolicyCatalogTests
{
    [Fact]
    public void ResolveRenewalPolicy_ShouldTranslateLeaseReferenceToSharedRequest()
    {
        var policy = new ApplicablePolicy(
            Guid.NewGuid(),
            "lease.renewal.default",
            "Default Lease Renewal Policy",
            "renewal",
            "lease",
            "Module",
            "lease",
            1,
            new DateOnly(2026, 1, 1),
            null,
            "lease.renewal.default",
            "renewal = enabled",
            new Dictionary<string, string>());
        var resolver = new SpyResolver(ApplicablePolicyResolution.Applicable(policy));
        var catalog = new LeasePolicyCatalog(resolver);
        var terms = RenewalTerms.Create(true, 30, "lease.renewal.default");

        var result = catalog.ResolveRenewalPolicy(
            terms.RenewalPolicyReference,
            "lease",
            new DateOnly(2026, 8, 9));

        Assert.True(result.IsApplicable);
        Assert.Same(policy, result.Policy);
        Assert.NotNull(resolver.Request);
        Assert.Equal("lease", resolver.Request.Consumer);
        Assert.Equal(terms.RenewalPolicyReference, resolver.Request.PolicyCode);
        Assert.Equal("renewal", resolver.Request.PolicyType);
        Assert.Equal("Module", resolver.Request.ScopeKind);
        Assert.Equal("lease", resolver.Request.ScopeKey);
        Assert.True(terms.AutoRenew);
        Assert.Equal(30, terms.NoticePeriodDays);
    }

    [Fact]
    public void ResolveRenewalPolicy_ShouldPreserveExplicitNotApplicableOutcome()
    {
        var resolver = new SpyResolver(ApplicablePolicyResolution.NotApplicable());
        var catalog = new LeasePolicyCatalog(resolver);

        var result = catalog.ResolveRenewalPolicy(
            "lease.renewal.missing",
            "lease",
            new DateOnly(2026, 8, 9));

        Assert.False(result.IsApplicable);
        Assert.Null(result.Policy);
    }

    private sealed class SpyResolver : IApplicablePolicyResolver
    {
        private readonly ApplicablePolicyResolution _resolution;

        public SpyResolver(ApplicablePolicyResolution resolution)
        {
            _resolution = resolution;
        }

        public ApplicablePolicyRequest? Request { get; private set; }

        public ApplicablePolicyResolution Resolve(ApplicablePolicyRequest request)
        {
            Request = request;
            return _resolution;
        }
    }
}
