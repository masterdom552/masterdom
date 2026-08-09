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
using Masterdom.Modules.Properties.Domain.Entities.Property;
using PropertyEntity = Masterdom.Modules.Properties.Domain.Entities.Property.Property;
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
        Assert.NotNull(scope.ServiceProvider.GetService<IInventoryStockLocationLookup>());

        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<ReceiveStockCommand, ExecutionResult<InventoryItemAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<AdjustStockCommand, ExecutionResult<InventoryItemAggregate>>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ICommandHandler<TransferInventoryCommand, ExecutionResult<InventoryItemAggregate>>>());
    }

    [Fact]
    public async Task InventoryEndpoints_ShouldCreateInventoryItem()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (_, srcLocId, _) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();

        var createResult = InventoryEndpoints.CreateInventoryItem(
            new InventoryEndpoints.CreateInventoryItemRequest(
                Guid.NewGuid(),
                srcLocId,
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

    [Fact]
    public async Task InventoryEndpoints_ShouldReceiveStock()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (propertyId, srcLocId, _) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();
        var receiveHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<ReceiveStockCommand, ExecutionResult<InventoryItemAggregate>>>();

        var createResult = createHandler.Handle(new CreateInventoryItemCommand(
            propertyId,
            srcLocId,
            "SKU-5566",
            "Water Filter",
            8m,
            DateTime.UtcNow));

        var receiveResult = InventoryEndpoints.ReceiveStock(
            createResult.Value!.Id.Value,
            new InventoryEndpoints.ReceiveStockRequest(2.5m),
            receiveHandler);

        var receiveResponse = await ExecuteAsync(receiveResult);
        Assert.Equal(StatusCodes.Status200OK, receiveResponse.StatusCode);

        using var receiveJson = JsonDocument.Parse(receiveResponse.Body!);
        Assert.Equal(10.5m, receiveJson.RootElement.GetProperty("quantityOnHand").GetDecimal());
    }

    [Fact]
    public async Task InventoryEndpoints_WithZeroReceivedQuantity_ShouldReturnValidationFailure()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (propertyId, srcLocId, _) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();
        var receiveHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<ReceiveStockCommand, ExecutionResult<InventoryItemAggregate>>>();

        var createResult = createHandler.Handle(new CreateInventoryItemCommand(
            propertyId,
            srcLocId,
            "SKU-6677",
            "Furnace Filter",
            8m,
            DateTime.UtcNow));

        var receiveResult = InventoryEndpoints.ReceiveStock(
            createResult.Value!.Id.Value,
            new InventoryEndpoints.ReceiveStockRequest(0m),
            receiveHandler);

        var receiveResponse = await ExecuteAsync(receiveResult);
        Assert.Equal(StatusCodes.Status400BadRequest, receiveResponse.StatusCode);
    }

    [Fact]
    public async Task InventoryEndpoints_ShouldAdjustStock()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (propertyId, srcLocId, _) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();
        var adjustHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<AdjustStockCommand, ExecutionResult<InventoryItemAggregate>>>();

        var createResult = createHandler.Handle(new CreateInventoryItemCommand(
            propertyId,
            srcLocId,
            "SKU-7788",
            "Door Hinge",
            8m,
            DateTime.UtcNow));

        var adjustResult = InventoryEndpoints.AdjustStock(
            createResult.Value!.Id.Value,
            new InventoryEndpoints.AdjustStockRequest(-2.5m),
            adjustHandler);

        var adjustResponse = await ExecuteAsync(adjustResult);
        Assert.Equal(StatusCodes.Status200OK, adjustResponse.StatusCode);

        using var adjustJson = JsonDocument.Parse(adjustResponse.Body!);
        Assert.Equal(5.5m, adjustJson.RootElement.GetProperty("quantityOnHand").GetDecimal());
    }

    [Fact]
    public async Task InventoryEndpoints_WithZeroAdjustmentQuantity_ShouldReturnValidationFailure()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (propertyId, srcLocId, _) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();
        var adjustHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<AdjustStockCommand, ExecutionResult<InventoryItemAggregate>>>();

        var createResult = createHandler.Handle(new CreateInventoryItemCommand(
            propertyId,
            srcLocId,
            "SKU-8899",
            "Door Handle",
            8m,
            DateTime.UtcNow));

        var adjustResult = InventoryEndpoints.AdjustStock(
            createResult.Value!.Id.Value,
            new InventoryEndpoints.AdjustStockRequest(0m),
            adjustHandler);

        var adjustResponse = await ExecuteAsync(adjustResult);
        Assert.Equal(StatusCodes.Status400BadRequest, adjustResponse.StatusCode);
    }

    [Fact]
    public async Task InventoryEndpoints_ShouldTransferInventory()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (propertyId, srcLocId, dstLocId) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();
        var transferHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<TransferInventoryCommand, ExecutionResult<InventoryItemAggregate>>>();

        var sku = "SKU-TRANSFER-9900";
        var source = createHandler.Handle(new CreateInventoryItemCommand(propertyId, srcLocId, sku, "Transfer Source", 8m, DateTime.UtcNow));
        createHandler.Handle(new CreateInventoryItemCommand(propertyId, dstLocId, sku, "Transfer Destination", 2m, DateTime.UtcNow));

        var transferResult = InventoryEndpoints.TransferInventory(
            source.Value!.Id.Value,
            new InventoryEndpoints.TransferInventoryRequest(dstLocId, 2.5m),
            transferHandler);

        var transferResponse = await ExecuteAsync(transferResult);
        Assert.Equal(StatusCodes.Status200OK, transferResponse.StatusCode);

        using var transferJson = JsonDocument.Parse(transferResponse.Body!);
        Assert.Equal(5.5m, transferJson.RootElement.GetProperty("quantityOnHand").GetDecimal());
    }

    [Fact]
    public async Task InventoryEndpoints_TransferToMissingDestinationItem_ShouldReturnConflict()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var (propertyId, srcLocId, dstLocId) = SeedStockLocations(scope);

        var createHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>>>();
        var transferHandler = scope.ServiceProvider
            .GetRequiredService<ICommandHandler<TransferInventoryCommand, ExecutionResult<InventoryItemAggregate>>>();

        var source = createHandler.Handle(new CreateInventoryItemCommand(propertyId, srcLocId, "SKU-NO-DEST", "Source Only", 8m, DateTime.UtcNow));

        var transferResult = InventoryEndpoints.TransferInventory(
            source.Value!.Id.Value,
            new InventoryEndpoints.TransferInventoryRequest(dstLocId, 2.5m),
            transferHandler);

        var transferResponse = await ExecuteAsync(transferResult);
        Assert.Equal(StatusCodes.Status409Conflict, transferResponse.StatusCode);
    }

    // Seeds two active stock locations for a new property and returns (PropertyId, SourceLocationId, DestLocationId)
    private static (Guid PropertyId, Guid SrcLocId, Guid DstLocId) SeedStockLocations(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<MasterdomDbContext>();

        var property = PropertyEntity.Create(
            new PropertyCode($"T{Guid.NewGuid():N}".Substring(0, 8).ToUpper()),
            new PropertyName("Test Property"),
            PropertyType.Commercial);

        var src = property.AddStockLocation("Source Storage", "SRC");
        var dst = property.AddStockLocation("Dest Storage", "DST");

        db.Properties.Add(property);
        db.SaveChanges();

        return (property.Id.Value, src.Id.Value, dst.Id.Value);
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
