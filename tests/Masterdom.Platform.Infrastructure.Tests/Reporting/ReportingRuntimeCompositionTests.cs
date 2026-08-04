using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Reporting.Application.Models;
using Masterdom.Modules.Reporting.Application.Queries;
using Masterdom.Modules.Reporting.Application.Support;
using Masterdom.Platform.ReadModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Masterdom.Platform.Infrastructure.Tests.Reporting;

public sealed class ReportingRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveReportingServicesAndHandler()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Reporting.Application.Services.IReportApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IReadModelProjectionOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<ITenancyReadModelProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPropertyReadModelProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMeteringReadModelProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<IBillingReadModelProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<IPaymentReadModelProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<IFinancialLedgerReadModelProvider>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Reporting.Application.Services.IReportExportService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GenerateReportQuery, ExecutionResult<GeneratedReport>>>());
    }

    [Fact]
    public async Task ReportingEndpoints_ShouldGenerateMonthlyDashboardReport()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<GenerateReportQuery, ExecutionResult<GeneratedReport>>>();

        var result = ReportingEndpoints.GenerateReport(
            new ReportingEndpoints.GenerateReportRequest(
                "monthly-dashboard",
                "period",
                false,
                1,
                10,
                Masterdom.Modules.Reporting.Application.Export.ReportExportFormat.Csv,
                "ops",
                true,
                new Dictionary<string, string>()),
            handler);

        var response = await ExecuteAsync(result);

        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        using var json = JsonDocument.Parse(response.Body!);
        Assert.Equal("monthly-dashboard", json.RootElement.GetProperty("reportCode").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"reporting-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "reporting-runtime-superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: Array.Empty<string>(),
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: Array.Empty<Guid>());
    }

    private static async Task<(int StatusCode, string? Body)> ExecuteAsync(IResult result)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();

        var context = new DefaultHttpContext();
        context.RequestServices = services.BuildServiceProvider();
        await using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        await result.ExecuteAsync(context);

        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, body);
    }

    private sealed class FixedCurrentUserAccessor : ICurrentUserAccessor
    {
        private readonly CurrentUser _currentUser;

        public FixedCurrentUserAccessor(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public CurrentUser GetCurrentUser() => _currentUser;
    }
}
