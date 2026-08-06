using System.Text.Json;
using Masterdom.Core.Security;
using Masterdom.Host.Api;
using Masterdom.Infrastructure;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Services;
using Masterdom.Modules.Inventory.Application.Support;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using Masterdom.Modules.Inventory.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Platform.Infrastructure.Tests.Inventory;

public sealed class InventoryRuntimeCompositionTests
{
    [Fact]
    public void AddPropertyBusinessCapabilityRuntime_ShouldResolveInventoryServicesAndHandlers()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<IInventoryApplicationService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IInventoryItemRepository>());
        Assert.NotNull(scope.ServiceProvider.GetService<IInventoryUnitOfWork>());
        Assert.NotNull(scope.ServiceProvider.GetService<IInventoryPlatformOrchestrator>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>());
    }

    [Fact]
    public async Task InventoryEndpoints_ShouldCreateInventoryItem()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();

        var createResult = InventoryEndpoints.CreateInventoryItem(
            new InventoryEndpoints.CreateInventoryItemRequest(
                Guid.NewGuid(),
                "SKU-4455",
                "Smoke Detector",
                8m,
                DateTime.UtcNow),
            createHandler);

        var createResponse = await ExecuteAsync(createResult);
        Assert.Equal(StatusCodes.Status201Created, createResponse.StatusCode);

        using var createJson = JsonDocument.Parse(createResponse.Body!);
        Assert.Equal("SKU-4455", createJson.RootElement.GetProperty("sku").GetString());
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        services.AddDbContext<MasterdomDbContext>(options =>
        {
            options.UseInMemoryDatabase($"inventory-runtime-{Guid.NewGuid():N}");
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
            username: "inventory-runtime-superuser",
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
