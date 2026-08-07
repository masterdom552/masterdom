using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Maintenance.Application.Commands;
using Masterdom.Modules.Maintenance.Application.Queries;
using Masterdom.Modules.Maintenance.Application.Services;
using Masterdom.Modules.Maintenance.Application.Support;
using Masterdom.Modules.Maintenance.Domain.Entities.Maintenance;
using Masterdom.Modules.Maintenance.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MaintenanceTicketAggregate = Masterdom.Modules.Maintenance.Domain.Entities.Maintenance.MaintenanceTicket;

namespace Masterdom.Platform.Infrastructure.Tests.Maintenance;

public sealed class MaintenanceRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveMaintenanceServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IMaintenanceApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMaintenanceTicketRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMaintenanceUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IMaintenancePlatformOrchestrator>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreateMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<AssignMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CloseMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<IQueryHandler<GetMaintenanceTicketByIdQuery, ExecutionResult<MaintenanceTicketAggregate>>>());
    }

    [Fact]
    public async Task MaintenanceEndpoints_ShouldCreateAndReadMaintenanceTicket()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>>();
        var assignHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<AssignMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>>();
        var closeHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CloseMaintenanceTicketCommand, ExecutionResult<MaintenanceTicketAggregate>>>();
        var getByIdHandler = scope.ServiceProvider
            .GetRequiredService<IQueryHandler<GetMaintenanceTicketByIdQuery, ExecutionResult<MaintenanceTicketAggregate>>>();

        var createResult = MaintenanceEndpoints.CreateMaintenanceTicket(
            new MaintenanceEndpoints.CreateMaintenanceTicketRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Door lock issue",
                "Main door lock is jammed.",
                DateTime.UtcNow),
            createHandler);

        var createResponse = await ExecuteAsync(createResult);
        Assert.Equal(StatusCodes.Status201Created, createResponse.StatusCode);

        using var createJson = JsonDocument.Parse(createResponse.Body!);
        var maintenanceTicketId = createJson.RootElement.GetProperty("id").GetGuid();

        var assignedToPersonId = Guid.NewGuid();
        var assignResult = MaintenanceEndpoints.AssignMaintenanceTicket(
            maintenanceTicketId,
            new MaintenanceEndpoints.AssignMaintenanceTicketRequest(
                assignedToPersonId,
                DateTime.UtcNow),
            assignHandler);

        var assignResponse = await ExecuteAsync(assignResult);
        Assert.Equal(StatusCodes.Status200OK, assignResponse.StatusCode);

        var closeResult = MaintenanceEndpoints.CloseMaintenanceTicket(
            maintenanceTicketId,
            new MaintenanceEndpoints.CloseMaintenanceTicketRequest(DateTime.UtcNow),
            closeHandler);

        var closeResponse = await ExecuteAsync(closeResult);
        Assert.Equal(StatusCodes.Status200OK, closeResponse.StatusCode);

        var getResult = MaintenanceEndpoints.GetMaintenanceTicketById(maintenanceTicketId, getByIdHandler);
        var getResponse = await ExecuteAsync(getResult);

        Assert.Equal(StatusCodes.Status200OK, getResponse.StatusCode);

        using var getJson = JsonDocument.Parse(getResponse.Body!);
        Assert.Equal("Door lock issue", getJson.RootElement.GetProperty("title").GetString());
        Assert.Equal(assignedToPersonId, getJson.RootElement.GetProperty("assignedToPersonId").GetGuid());
        Assert.Equal("Closed", getJson.RootElement.GetProperty("status").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"maintenance-runtime-{Guid.NewGuid():N}");
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
            username: "maintenance-runtime-superuser",
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

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

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
