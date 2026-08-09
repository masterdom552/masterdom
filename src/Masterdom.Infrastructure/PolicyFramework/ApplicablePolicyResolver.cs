using Masterdom.Abstractions.Policies;
using Masterdom.Modules.PolicyFramework.Application.Queries;
using Masterdom.Modules.PolicyFramework.Application.Services;
using Masterdom.Modules.PolicyFramework.Domain.Entities.PolicyFramework;

namespace Masterdom.Infrastructure.PolicyFramework;

internal sealed class ApplicablePolicyResolver : IApplicablePolicyResolver
{
    private readonly IPolicyFrameworkApplicationService _applicationService;

    public ApplicablePolicyResolver(IPolicyFrameworkApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ApplicablePolicyResolution Resolve(ApplicablePolicyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Consumer);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PolicyCode);

        var policy = _applicationService.GetApplicablePolicy(new GetApplicablePolicyQuery(
            PolicyType.Create(request.PolicyType),
            PolicyScope.Create(PolicyScopeKind.Create(request.ScopeKind), request.ScopeKey),
            request.AsOfDate,
            request.PolicyCode));

        if (policy is null || !string.Equals(
                policy.PolicyReference.PolicyCode,
                request.PolicyCode,
                StringComparison.OrdinalIgnoreCase))
        {
            return ApplicablePolicyResolution.NotApplicable();
        }

        var version = policy.ResolveApplicableVersion(
            PolicyScope.Create(PolicyScopeKind.Create(request.ScopeKind), request.ScopeKey),
            request.AsOfDate);

        if (version is null)
        {
            return ApplicablePolicyResolution.NotApplicable();
        }

        return ApplicablePolicyResolution.Applicable(new ApplicablePolicy(
            policy.Id.Value,
            policy.PolicyReference.PolicyCode,
            policy.PolicyReference.DisplayName,
            policy.PolicyType.Value,
            policy.PolicyCategory.Value,
            policy.Scope.Kind.Value,
            policy.Scope.ScopeKey,
            version.VersionNumber,
            version.EffectiveDateRange.StartDate,
            version.EffectiveDateRange.EndDate,
            version.Condition.SelectorKey,
            version.Condition.SelectorDefinition,
            new Dictionary<string, string>(version.Metadata.Attributes, StringComparer.OrdinalIgnoreCase)));
    }
}
