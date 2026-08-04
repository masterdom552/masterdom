using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Metering;
using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Queries;
using Masterdom.Modules.Metering.Application.Services;
using Masterdom.Modules.Metering.Application.Support;
using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Metering.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Platform.Infrastructure.Tests.Metering;

public sealed class MeteringRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveMeteringServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IMeteringApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMeterRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMeteringUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMeteringPlatformOrchestrator>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<InstallMeterCommand, ExecutionResult<MeterAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<SubmitReadingCommand, ExecutionResult<MeterAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ApproveReadingCommand, ExecutionResult<MeterAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CorrectReadingCommand, ExecutionResult<MeterAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<RetireMeterCommand, ExecutionResult<MeterAggregate>>>());

        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetMeterByIdQuery, ExecutionResult<MeterAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetMeterByNumberQuery, ExecutionResult<MeterAggregate>>>());
    }

    [Fact]
    public async Task MeteringEndpoints_ShouldInstallAndReadMeter()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var installHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<InstallMeterCommand, ExecutionResult<MeterAggregate>>>();
        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetMeterByIdQuery, ExecutionResult<MeterAggregate>>>();

        var installResult = MeteringEndpoints.InstallMeter(
            new MeteringEndpoints.InstallMeterRequest(
                "MTR-INT-01",
                "Electricity",
                "Smart",
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))),
            installHandler);

        var installResponse = await ExecuteAsync(installResult);
        Assert.Equal(StatusCodes.Status201Created, installResponse.StatusCode);

        using var installJson = JsonDocument.Parse(installResponse.Body!);
        var meterId = installJson.RootElement.GetProperty("id").GetGuid();

        var getResult = MeteringEndpoints.GetMeterById(meterId, getByIdHandler);
        var getResponse = await ExecuteAsync(getResult);

        Assert.Equal(StatusCodes.Status200OK, getResponse.StatusCode);
        using var getJson = JsonDocument.Parse(getResponse.Body!);
        Assert.Equal("MTR-INT-01", getJson.RootElement.GetProperty("meterNumber").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"metering-runtime-{Guid.NewGuid():N}");
        });

        services.AddPropertyBusinessCapabilityRuntime();
        services.AddScoped<ICurrentUserAccessor>(_ => new FixedCurrentUserAccessor(CreateSuperUser()));
        services.AddScoped<IMeteringUnitOfWork, PassThroughMeteringUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static CurrentUser CreateSuperUser()
    {
        return CurrentUser.Authenticated(
            userId: Guid.NewGuid(),
            personId: null,
            username: "metering-runtime-superuser",
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

    private sealed class PassThroughMeteringUnitOfWork : IMeteringUnitOfWork
    {
        private readonly MasterdomDbContext _dbContext;

        public PassThroughMeteringUnitOfWork(MasterdomDbContext dbContext)
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
