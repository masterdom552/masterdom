using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Payment.Application.Commands;
using Masterdom.Modules.Payment.Application.Queries;
using Masterdom.Modules.Payment.Application.Support;
using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.Payment.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Platform.Infrastructure.Tests.Payment;

public sealed class PaymentRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolvePaymentServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IPaymentRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Payment.Application.Support.IPaymentUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Payment.Application.Support.IPaymentPlatformOrchestrator>());
        Assert.NotNull(scope.ServiceProvider.GetService<Masterdom.Modules.Payment.Application.Services.IPaymentApplicationService>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ReceivePaymentCommand, ExecutionResult<PaymentAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<AllocatePaymentCommand, ExecutionResult<PaymentAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ReversePaymentCommand, ExecutionResult<PaymentAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<VoidPaymentCommand, ExecutionResult<PaymentAggregate>>>());

        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetPaymentByIdQuery, ExecutionResult<PaymentAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetPaymentByReferenceQuery, ExecutionResult<PaymentAggregate>>>());
    }

    [Fact]
    public async Task PaymentEndpoints_ShouldReceiveAndReadPayment()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var receiveHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<ReceivePaymentCommand, ExecutionResult<PaymentAggregate>>>();
        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetPaymentByIdQuery, ExecutionResult<PaymentAggregate>>>();

        var receiveResult = PaymentEndpoints.ReceivePayment(
            new PaymentEndpoints.ReceivePaymentRequest(
                "PAY-INT-01",
                500m,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "BankTransfer",
                "Counter",
                "Tenant",
                DateTime.UtcNow),
            receiveHandler);

        var receiveResponse = await ExecuteAsync(receiveResult);
        Assert.Equal(StatusCodes.Status201Created, receiveResponse.StatusCode);

        using var receiveJson = JsonDocument.Parse(receiveResponse.Body!);
        var paymentId = receiveJson.RootElement.GetProperty("id").GetGuid();

        var getResult = PaymentEndpoints.GetPaymentById(paymentId, getByIdHandler);
        var getResponse = await ExecuteAsync(getResult);

        Assert.Equal(StatusCodes.Status200OK, getResponse.StatusCode);
        using var getJson = JsonDocument.Parse(getResponse.Body!);
        Assert.Equal("PAY-INT-01", getJson.RootElement.GetProperty("paymentReference").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"payment-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));
        services.AddScoped<Masterdom.Modules.Payment.Application.Support.IPaymentUnitOfWork, PassThroughPaymentUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "payment-runtime-superuser",
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

    private sealed class PassThroughPaymentUnitOfWork : Masterdom.Modules.Payment.Application.Support.IPaymentUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughPaymentUnitOfWork(MasterdomDbContext dbContext)
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
