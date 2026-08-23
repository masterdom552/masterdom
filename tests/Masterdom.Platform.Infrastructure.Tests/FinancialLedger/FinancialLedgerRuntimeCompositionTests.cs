using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using Masterdom.Modules.FinancialLedger.Application.Support;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Platform.Infrastructure.Tests.FinancialLedger;

public sealed class FinancialLedgerRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveFinancialLedgerServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ILedgerRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.FinancialLedger.Application.Support.ILedgerUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.FinancialLedger.Application.Support.ILedgerPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.FinancialLedger.Application.Services.ILedgerApplicationService>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<OpenLedgerCommand, ExecutionResult<LedgerAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<PostBillingJournalCommand, ExecutionResult<LedgerAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<PostPaymentJournalCommand, ExecutionResult<LedgerAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ReverseJournalCommand, ExecutionResult<LedgerAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CompletePostingBatchCommand, ExecutionResult<LedgerAggregate>>>());

        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetLedgerByIdQuery, ExecutionResult<LedgerAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetLedgerByCodeQuery, ExecutionResult<LedgerAggregate>>>());
    }

    [Fact]
    public async Task FinancialLedgerEndpoints_ShouldOpenAndReadLedger()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var openHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<OpenLedgerCommand, ExecutionResult<LedgerAggregate>>>();
        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetLedgerByIdQuery, ExecutionResult<LedgerAggregate>>>();

        var openResult = FinancialLedgerEndpoints.OpenLedger(
            new FinancialLedgerEndpoints.OpenLedgerRequest("GL-INT-01", "Integration Ledger", DateTime.UtcNow),
            openHandler);

        var openResponse = await ExecuteAsync(openResult);
        Assert.Equal(StatusCodes.Status201Created, openResponse.StatusCode);

        using var openJson = JsonDocument.Parse(openResponse.Body!);
        var ledgerId = openJson.RootElement.GetProperty("id").GetGuid();

        var getResult = FinancialLedgerEndpoints.GetLedgerById(ledgerId, getByIdHandler);
        var getResponse = await ExecuteAsync(getResult);

        Assert.Equal(StatusCodes.Status200OK, getResponse.StatusCode);
        using var getJson = JsonDocument.Parse(getResponse.Body!);
        Assert.Equal("GL-INT-01", getJson.RootElement.GetProperty("ledgerCode").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"financial-ledger-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));
        services.AddScoped<Masterdom.Modules.FinancialLedger.Application.Support.ILedgerUnitOfWork, PassThroughLedgerUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "ledger-runtime-superuser",
            roles: [MasterdomRoles.SuperUser],
            permissions: Array.Empty<string>(),
            propertyScopes: Array.Empty<Guid>(),
            ownedPropertyIds: Array.Empty<Guid>(),
            isInherentSuperUser: true);
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

    private sealed class PassThroughLedgerUnitOfWork : Masterdom.Modules.FinancialLedger.Application.Support.ILedgerUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughLedgerUnitOfWork(MasterdomDbContext dbContext)
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
