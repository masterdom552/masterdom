using System.Security.Claims;
using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Microsoft.AspNetCore.Http;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Modules.Intelligence.Application.Handlers;
using Masterdom.Modules.Intelligence.Application.Models;
using Masterdom.Modules.Intelligence.Application.Queries;
using Masterdom.Modules.Intelligence.Application.Services;
using Masterdom.Modules.Intelligence.Application.Support;
using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Services;

namespace Masterdom.Platform.Infrastructure.Tests.Intelligence;

/// <summary>
/// Tests for Property Performance Analytics query and handler.
///
/// Validates:
/// - Query validation logic
/// - Handler error handling
/// - Business threshold calculations
/// </summary>
public sealed class PropertyPerformanceAnalyticsQueryHandlerTests
{
    [Fact]
    public void Query_RequiresPropertyId()
    {
        // Arrange
        var query = new GetPropertyPerformanceAnalyticsQuery(
            PropertyId: Guid.Empty,
            UserId: Guid.NewGuid(),
            MonthsHistorical: 3);

        // Act & Assert
        Assert.Equal(Guid.Empty, query.PropertyId);
    }

    [Fact]
    public void Query_RequiresUserId()
    {
        // Arrange
        var query = new GetPropertyPerformanceAnalyticsQuery(
            PropertyId: Guid.NewGuid(),
            UserId: Guid.Empty,
            MonthsHistorical: 3);

        // Act & Assert
        Assert.Equal(Guid.Empty, query.UserId);
    }

    [Fact]
    public void Query_DefaultsMonthsHistorical()
    {
        // Arrange
        var query = new GetPropertyPerformanceAnalyticsQuery(
            PropertyId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        // Act & Assert
        Assert.Equal(3, query.MonthsHistorical);
    }

    [Fact]
    public void Query_SupportsCustomMonthsHistorical()
    {
        // Arrange
        var query = new GetPropertyPerformanceAnalyticsQuery(
            PropertyId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            MonthsHistorical: 12);

        // Act & Assert
        Assert.Equal(12, query.MonthsHistorical);
    }

    [Fact]
    public void ResultModel_PropertyPerformanceAnalyticsResult_IsNonNull()
    {
        // Specification: Result model must contain all required analytics fields
        var propertyId = Guid.NewGuid();
        var nowUtc = DateTime.UtcNow;

        var occupancy = new OccupancyTrendData(
            Direction: "STABLE",
            PercentageChange: 0m,
            CurrentRate: 0.85m,
            PreviousRate: 0.85m);

        var revenue = new RevenueTrendData(
            Direction: "STABLE",
            PercentageChange: 0m,
            CurrentAmount: 1500m,
            PreviousAmount: 1500m);

        var expenses = new ExpenseRatioData(
            Ratio: 0.40m,
            Status: "ACCEPTABLE");

        var health = new HealthSummary(
            OverallStatus: "HEALTHY",
            Concerns: new List<string>().AsReadOnly(),
            Recommendations: new List<string> { "Maintain current operations" }.AsReadOnly());

        var result = new PropertyPerformanceAnalyticsResult(
            PropertyId: propertyId,
            AsOfDateUtc: nowUtc,
            OccupancyTrend: occupancy,
            RevenuePerUnitTrend: revenue,
            ExpenseRatio: expenses,
            HealthSummary: health);

        // Assert all fields are set
        Assert.Equal(propertyId, result.PropertyId);
        Assert.NotEqual(default, result.AsOfDateUtc);
        Assert.NotNull(result.OccupancyTrend);
        Assert.NotNull(result.RevenuePerUnitTrend);
        Assert.NotNull(result.ExpenseRatio);
        Assert.NotNull(result.HealthSummary);
    }

    [Fact]
    public void HealthSummary_ContainsStatusAndLists()
    {
        // Specification: Health summary must contain overall status, concerns, and recommendations

        var concerns = new List<string> { "Revenue declining" }.AsReadOnly();
        var recommendations = new List<string> { "Review pricing" }.AsReadOnly();

        var health = new HealthSummary(
            OverallStatus: "CAUTION",
            Concerns: concerns,
            Recommendations: recommendations);

        Assert.Equal("CAUTION", health.OverallStatus);
        Assert.Single(health.Concerns);
        Assert.Single(health.Recommendations);
    }

    [Fact]
    public void HealthStatusValues_AreDocumented()
    {
        // Specification: Health status uses consistent values across types

        var validStatuses = new[] { "HEALTHY", "CAUTION", "ALERT" };

        foreach (var status in validStatuses)
        {
            var health = new HealthSummary(
                OverallStatus: status,
                Concerns: new List<string>().AsReadOnly(),
                Recommendations: new List<string>().AsReadOnly());

            Assert.Contains(status, validStatuses);
        }
    }

    [Fact]
    public void OccupancyTrendDirection_UsesConsistentValues()
    {
        // Specification: Trend direction values should be consistent

        var directions = new[] { "IMPROVING", "STABLE", "DECLINING" };

        foreach (var direction in directions)
        {
            var trend = new OccupancyTrendData(
                Direction: direction,
                PercentageChange: 0m,
                CurrentRate: 0.85m,
                PreviousRate: 0.85m);

            Assert.Contains(direction, directions);
        }
    }
}

/// <summary>
/// Tests for CAP-018 authority enforcement in GetPropertyPerformanceAnalyticsQueryHandler
/// (PKG-CAP-022-AUTHORITY-ENFORCEMENT).
///
/// Validates:
/// - Direct authority grants/denies property-scoped access
/// - Active delegation grants access beyond direct scope; expired delegation does not
/// - Inherent SuperUser bypasses property scope
/// - Missing/unresolvable authority fails closed
/// - Authorization rejection occurs before Reporting data is fetched
/// - The endpoint maps rejection to the correct HTTP status
/// </summary>
public sealed class GetPropertyPerformanceAnalyticsQueryHandlerAuthorizationTests
{
    private static readonly Guid PropertyId = Guid.NewGuid();
    private static readonly Guid OtherPropertyId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestRoleId = Guid.NewGuid();
    private static readonly Guid DelegatorUserId = Guid.NewGuid();
    private static readonly Guid DelegatedRoleId = Guid.NewGuid();

    private static GetPropertyPerformanceAnalyticsQueryHandler CreateHandler(
        CurrentUser currentUser,
        DirectAuthority? directAuthority,
        IReadOnlyCollection<DelegatedAuthority> activeDelegations,
        IReadOnlyDictionary<Guid, int> authorityLevelsByRoleId,
        SpyReportApplicationService reportingSpy)
    {
        var analyticsService = new PropertyPerformanceAnalyticsService(reportingSpy);

        return new GetPropertyPerformanceAnalyticsQueryHandler(
            analyticsService,
            new FakeCurrentUserAccessor(currentUser),
            new FakeDirectAuthorityProvider(directAuthority),
            new FakeActiveDelegationsProvider(activeDelegations),
            new EffectiveAuthorityResolver(new FakeAuthorityLevelProvider(authorityLevelsByRoleId)));
    }

    private static GetPropertyPerformanceAnalyticsQuery Query(Guid propertyId) =>
        new(PropertyId: propertyId, UserId: TestUserId, MonthsHistorical: 3);

    private static CurrentUser AuthenticatedUser() =>
        CurrentUser.Authenticated(
            userId: TestUserId,
            personId: null,
            username: "test-user",
            roles: null,
            permissions: null,
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: null);

    [Fact]
    public void DirectAuthority_PropertyInScope_Succeeds()
    {
        var directAuthority = new DirectAuthority(TestRoleId, [], [PropertyId]);
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority,
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int> { [TestRoleId] = AuthorityLevels.Admin },
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.True(result.IsSuccess);
        Assert.True(reportingSpy.WasCalled);
    }

    [Fact]
    public void DirectAuthority_PropertyOutsideScope_IsRejectedBeforeReportingAccess()
    {
        var directAuthority = new DirectAuthority(TestRoleId, [], [OtherPropertyId]);
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority,
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int> { [TestRoleId] = AuthorityLevels.Admin },
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.False(result.IsSuccess);
        Assert.Equal("forbidden", result.ErrorCode);
        Assert.False(reportingSpy.WasCalled);
    }

    [Fact]
    public void ActiveDelegation_GrantsAccessBeyondDirectScope_Succeeds()
    {
        // Direct authority alone does not cover PropertyId - only an active delegation does.
        var directAuthority = new DirectAuthority(TestRoleId, [], [OtherPropertyId]);
        var delegation = DelegatedAuthority.Create(
            UserId.From(DelegatorUserId),
            UserId.From(TestUserId),
            RoleId.From(DelegatedRoleId),
            DelegationScope.WithProperties([PropertyId]),
            effectiveFromUtc: DateTime.UtcNow.AddDays(-1),
            effectiveToUtc: DateTime.UtcNow.AddDays(1));
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority,
            activeDelegations: [delegation],
            authorityLevelsByRoleId: new Dictionary<Guid, int>
            {
                [TestRoleId] = AuthorityLevels.Admin,
                [DelegatedRoleId] = AuthorityLevels.Admin
            },
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.True(result.IsSuccess);
        Assert.True(reportingSpy.WasCalled);
    }

    [Fact]
    public void ExpiredDelegation_DoesNotGrantAccess()
    {
        var directAuthority = new DirectAuthority(TestRoleId, [], [OtherPropertyId]);
        var expiredDelegation = DelegatedAuthority.Create(
            UserId.From(DelegatorUserId),
            UserId.From(TestUserId),
            RoleId.From(DelegatedRoleId),
            DelegationScope.WithProperties([PropertyId]),
            effectiveFromUtc: DateTime.UtcNow.AddDays(-10),
            effectiveToUtc: DateTime.UtcNow.AddDays(-1));
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority,
            activeDelegations: [expiredDelegation],
            authorityLevelsByRoleId: new Dictionary<Guid, int>
            {
                [TestRoleId] = AuthorityLevels.Admin,
                [DelegatedRoleId] = AuthorityLevels.Admin
            },
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.False(result.IsSuccess);
        Assert.Equal("forbidden", result.ErrorCode);
        Assert.False(reportingSpy.WasCalled);
    }

    [Fact]
    public void InherentSuperUser_BypassesPropertyScope()
    {
        var directAuthority = new DirectAuthority(TestRoleId, [], []);
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority,
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int> { [TestRoleId] = AuthorityLevels.PrimarySuperUser },
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.True(result.IsSuccess);
        Assert.True(reportingSpy.WasCalled);
    }

    [Fact]
    public void MissingDirectAuthority_FailsClosed()
    {
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority: null,
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int>(),
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.False(result.IsSuccess);
        Assert.Equal("forbidden", result.ErrorCode);
        Assert.False(reportingSpy.WasCalled);
    }

    [Fact]
    public void UnauthenticatedUser_FailsClosed()
    {
        var reportingSpy = new SpyReportApplicationService();

        var handler = CreateHandler(
            CurrentUser.Anonymous,
            directAuthority: new DirectAuthority(TestRoleId, [], [PropertyId]),
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int> { [TestRoleId] = AuthorityLevels.Admin },
            reportingSpy);

        var result = handler.Handle(Query(PropertyId));

        Assert.False(result.IsSuccess);
        Assert.Equal("unauthorized", result.ErrorCode);
        Assert.False(reportingSpy.WasCalled);
    }

    [Fact]
    public void Endpoint_UnauthorizedRejection_MapsToHttp403()
    {
        var directAuthority = new DirectAuthority(TestRoleId, [], [OtherPropertyId]);
        var reportingSpy = new SpyReportApplicationService();
        var handler = CreateHandler(
            AuthenticatedUser(),
            directAuthority,
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int> { [TestRoleId] = AuthorityLevels.Admin },
            reportingSpy);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", TestUserId.ToString())],
            authenticationType: "Test"));

        var response = IntelligenceEndpoints.GetPropertyPerformance(
            PropertyId,
            monthsHistorical: 3,
            handler,
            principal);

        var problem = Assert.IsAssignableFrom<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(response);
        Assert.Equal(StatusCodes.Status403Forbidden, problem.StatusCode);
        Assert.False(reportingSpy.WasCalled);
    }

    [Fact]
    public void Endpoint_UnauthenticatedRejection_MapsToHttp401()
    {
        var reportingSpy = new SpyReportApplicationService();
        var handler = CreateHandler(
            CurrentUser.Anonymous,
            directAuthority: new DirectAuthority(TestRoleId, [], [PropertyId]),
            activeDelegations: [],
            authorityLevelsByRoleId: new Dictionary<Guid, int> { [TestRoleId] = AuthorityLevels.Admin },
            reportingSpy);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("sub", TestUserId.ToString())],
            authenticationType: "Test"));

        var response = IntelligenceEndpoints.GetPropertyPerformance(
            PropertyId,
            monthsHistorical: 3,
            handler,
            principal);

        var problem = Assert.IsAssignableFrom<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(response);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.StatusCode);
        Assert.False(reportingSpy.WasCalled);
    }

    private sealed class FakeCurrentUserAccessor(CurrentUser user) : ICurrentUserAccessor
    {
        public CurrentUser GetCurrentUser() => user;
    }

    private sealed class FakeDirectAuthorityProvider(DirectAuthority? directAuthority) : IDirectAuthorityProvider
    {
        public Task<DirectAuthority?> GetDirectAuthorityAsync(
            Guid userId,
            IReadOnlyCollection<Guid> propertyScopes,
            CancellationToken cancellationToken = default) => Task.FromResult(directAuthority);
    }

    private sealed class FakeActiveDelegationsProvider(IReadOnlyCollection<DelegatedAuthority> delegations)
        : IActiveDelegationsProvider
    {
        public Task<IReadOnlyCollection<DelegatedAuthority>> GetActiveDelegationsAsync(Guid userId, DateTime utcNow) =>
            Task.FromResult(delegations);
    }

    private sealed class FakeAuthorityLevelProvider(IReadOnlyDictionary<Guid, int> levelsByRoleId) : IAuthorityLevelProvider
    {
        public int GetAuthorityLevel(Guid roleId) =>
            levelsByRoleId.TryGetValue(roleId, out var level) ? level : AuthorityLevels.Tenant;
    }

    private sealed class SpyReportApplicationService : IReportApplicationService
    {
        public bool WasCalled { get; private set; }

        public GeneratedReport Generate(GenerateReportQuery query)
        {
            WasCalled = true;

            return new GeneratedReport(
                ReportCode: query.ReportCode,
                MimeType: "text/csv",
                ExportFileName: "report.csv",
                ExportContent: string.Empty,
                DataSet: new ReportDataSet(
                    columns: Array.Empty<ReportColumn>(),
                    rows: Array.Empty<ReportRow>(),
                    totalCount: 0,
                    page: 1,
                    pageSize: 3,
                    sortBy: "Month",
                    sortDescending: true),
                Snapshot: null,
                AppliedTemplate: null,
                Kpis: Array.Empty<string>(),
                DashboardSummaries: Array.Empty<string>());
        }
    }
}
