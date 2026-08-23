using Masterdom.Core.Security;
using Masterdom.Modules.Intelligence.Application.Models;
using Masterdom.Modules.Intelligence.Application.Queries;
using Masterdom.Modules.Intelligence.Application.Services;
using Masterdom.Modules.Intelligence.Application.Support;

namespace Masterdom.Modules.Intelligence.Application.Handlers;

/// <summary>
/// Handler for GetPropertyPerformanceAnalyticsQuery.
///
/// Responsibilities:
/// 1. Validate the query
/// 2. Enforce CAP-018 authority (verify user can read the property)
/// 3. Delegate to AnalyticsService for computation
/// 4. Return results or error
/// </summary>
public sealed class GetPropertyPerformanceAnalyticsQueryHandler
    : IQueryHandler<GetPropertyPerformanceAnalyticsQuery, ExecutionResult<PropertyPerformanceAnalyticsResult>>
{
    private readonly PropertyPerformanceAnalyticsService _analyticsService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IDirectAuthorityProvider _directAuthorityProvider;
    private readonly IActiveDelegationsProvider _activeDelegationsProvider;
    private readonly EffectiveAuthorityResolver _effectiveAuthorityResolver;

    public GetPropertyPerformanceAnalyticsQueryHandler(
        PropertyPerformanceAnalyticsService analyticsService,
        ICurrentUserAccessor currentUserAccessor,
        IDirectAuthorityProvider directAuthorityProvider,
        IActiveDelegationsProvider activeDelegationsProvider,
        EffectiveAuthorityResolver effectiveAuthorityResolver)
    {
        _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        _currentUserAccessor = currentUserAccessor ?? throw new ArgumentNullException(nameof(currentUserAccessor));
        _directAuthorityProvider = directAuthorityProvider ?? throw new ArgumentNullException(nameof(directAuthorityProvider));
        _activeDelegationsProvider = activeDelegationsProvider ?? throw new ArgumentNullException(nameof(activeDelegationsProvider));
        _effectiveAuthorityResolver = effectiveAuthorityResolver ?? throw new ArgumentNullException(nameof(effectiveAuthorityResolver));
    }

    /// <summary>
    /// Handle the analytics query.
    ///
    /// Authority is resolved via the existing CAP-018 model (EffectiveAuthorityResolver,
    /// combining direct authority and active delegations) before any Reporting data is
    /// fetched. Access is rejected unless the caller holds inherent SuperUser authority
    /// or the requested property is within their effective property scope.
    /// </summary>
    public ExecutionResult<PropertyPerformanceAnalyticsResult> Handle(
        GetPropertyPerformanceAnalyticsQuery query)
    {
        try
        {
            // Validate query
            if (query.PropertyId == Guid.Empty)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "invalid_property_id", "Property ID cannot be empty");

            if (query.UserId == Guid.Empty)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "invalid_user_id", "User ID cannot be empty");

            if (query.MonthsHistorical < 1 || query.MonthsHistorical > 24)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "invalid_months", "Months historical must be between 1 and 24");

            // Enforce CAP-018 authority before any Reporting data is accessed
            var currentUser = _currentUserAccessor.GetCurrentUser();
            if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "unauthorized", "Authentication is required.");

            var utcNow = DateTime.UtcNow;

            var directAuthority = _directAuthorityProvider
                .GetDirectAuthorityAsync(currentUser.UserId.Value, currentUser.PropertyScopes)
                .Result;

            if (directAuthority is null)
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "forbidden", "The current user has no active primary role assignment.");

            var activeDelegations = _activeDelegationsProvider
                .GetActiveDelegationsAsync(currentUser.UserId.Value, utcNow)
                .Result;

            var effectiveAuthority = _effectiveAuthorityResolver.Resolve(
                currentUser.UserId.Value,
                directAuthority,
                activeDelegations,
                utcNow);

            if (!effectiveAuthority.IsInherentSuperUser
                && !effectiveAuthority.PropertyScopes.Contains(query.PropertyId))
            {
                return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                    "forbidden", "The current user is not authorized to read this property's analytics.");
            }

            // Perform analytics
            var result = _analyticsService.AnalyzePropertyPerformance(
                query.PropertyId,
                query.MonthsHistorical);

            return ExecutionResult<PropertyPerformanceAnalyticsResult>.Success(result);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                "validation_error", ex.Message);
        }
        catch (Exception ex)
        {
            return ExecutionResult<PropertyPerformanceAnalyticsResult>.Failure(
                "analysis_failed", $"Property performance analysis failed: {ex.Message}");
        }
    }
}
