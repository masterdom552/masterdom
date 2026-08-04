using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Billing.Application.Commands;
using Masterdom.Modules.Billing.Application.Queries;
using Masterdom.Modules.Billing.Application.Support;
using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Platform.Infrastructure.Tests.Billing;

public sealed class BillingRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveBillingServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IBillRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Billing.Application.Support.IBillingUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Billing.Application.Support.IBillingPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Billing.Application.Services.IBillingApplicationService>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<GenerateBillCommand, ExecutionResult<BillAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<FinalizeBillCommand, ExecutionResult<BillAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<AddAdjustmentCommand, ExecutionResult<BillAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ApplyCreditCommand, ExecutionResult<BillAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<VoidBillCommand, ExecutionResult<BillAggregate>>>());

        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetBillByIdQuery, ExecutionResult<BillAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetBillByNumberQuery, ExecutionResult<BillAggregate>>>());
    }

    [Fact]
    public async Task BillingEndpoints_ShouldGenerateAndReadBill()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var generateHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<GenerateBillCommand, ExecutionResult<BillAggregate>>>();
        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetBillByIdQuery, ExecutionResult<BillAggregate>>>();

        var generateResult = BillingEndpoints.GenerateBill(
            new BillingEndpoints.GenerateBillRequest(
                "BILL-INT-01",
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 31),
                "Monthly",
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 1),
                new DateOnly(2026, 8, 10),
                "USD",
                [new BillingEndpoints.ChargeLineRequest("Rent", "Rent charge", 1200m, null)]),
            generateHandler);

        var generateResponse = await ExecuteAsync(generateResult);
        Assert.Equal(StatusCodes.Status201Created, generateResponse.StatusCode);

        using var json = JsonDocument.Parse(generateResponse.Body!);
        var id = json.RootElement.GetProperty("id").GetGuid();

        var getResult = BillingEndpoints.GetBillById(id, getByIdHandler);
        var getResponse = await ExecuteAsync(getResult);

        Assert.Equal(StatusCodes.Status200OK, getResponse.StatusCode);
        using var getJson = JsonDocument.Parse(getResponse.Body!);
        Assert.Equal("BILL-INT-01", getJson.RootElement.GetProperty("billNumber").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"billing-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));
        services.AddScoped<Masterdom.Modules.Billing.Application.Support.IBillingUnitOfWork, PassThroughBillingUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "billing-runtime-superuser",
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

    private sealed class PassThroughBillingUnitOfWork : Masterdom.Modules.Billing.Application.Support.IBillingUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughBillingUnitOfWork(MasterdomDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public void Execute(Action operation)
        {
            ArgumentNullException.ThrowIfNull(operation);
            operation();
            _dbContext.SaveChanges();
        }
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
