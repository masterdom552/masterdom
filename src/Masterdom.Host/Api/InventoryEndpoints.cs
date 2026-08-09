using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Support;
using Masterdom.Modules.Inventory.Domain.Entities.Inventory;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Host.Api;

internal static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/inventory/items").WithTags("Inventory").RequireAuthorization();

        group.MapPost("/", CreateInventoryItem);
        group.MapPost("/{inventoryItemId:guid}/receive", ReceiveStock);
        group.MapPost("/{inventoryItemId:guid}/adjust", AdjustStock);
        group.MapPost("/{inventoryItemId:guid}/transfer", TransferInventory);

        return app;
    }

    internal static IResult CreateInventoryItem(
        CreateInventoryItemRequest request,
        ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>> handler)
    {
        var command = new CreateInventoryItemCommand(
            request.PropertyId,
            request.StockLocationId,
            request.Sku,
            request.Name,
            request.QuantityOnHand,
            request.CreatedAtUtc);

        var result = handler.Handle(command);
        if (!result.IsSuccess || result.Value is null)
        {
            return ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
        }

        var response = InventoryItemResponse.From(result.Value);
        return TypedResults.Created($"/api/inventory/items/{response.Id}", response);
    }

    internal static IResult ReceiveStock(
        Guid inventoryItemId,
        ReceiveStockRequest request,
        ICommandHandler<ReceiveStockCommand, ExecutionResult<InventoryItemAggregate>> handler)
    {
        var command = new ReceiveStockCommand(
            InventoryItemId.From(inventoryItemId),
            request.ReceivedQuantity);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(InventoryItemResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult AdjustStock(
        Guid inventoryItemId,
        AdjustStockRequest request,
        ICommandHandler<AdjustStockCommand, ExecutionResult<InventoryItemAggregate>> handler)
    {
        var command = new AdjustStockCommand(
            InventoryItemId.From(inventoryItemId),
            request.AdjustmentQuantity);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(InventoryItemResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal static IResult TransferInventory(
        Guid inventoryItemId,
        TransferInventoryRequest request,
        ICommandHandler<TransferInventoryCommand, ExecutionResult<InventoryItemAggregate>> handler)
    {
        var command = new TransferInventoryCommand(
            InventoryItemId.From(inventoryItemId),
            request.DestinationStockLocationId,
            request.TransferQuantity);

        var result = handler.Handle(command);
        return result.IsSuccess && result.Value is not null
            ? TypedResults.Ok(InventoryItemResponse.From(result.Value))
            : ApiExecutionResults.ToErrorResult(result.ErrorCode, result.ErrorMessage);
    }

    internal sealed record CreateInventoryItemRequest(
        Guid PropertyId,
        Guid StockLocationId,
        string Sku,
        string Name,
        decimal QuantityOnHand,
        DateTime CreatedAtUtc);

    internal sealed record ReceiveStockRequest(decimal ReceivedQuantity);

    internal sealed record AdjustStockRequest(decimal AdjustmentQuantity);

    internal sealed record TransferInventoryRequest(
        Guid DestinationStockLocationId,
        decimal TransferQuantity);

    internal sealed record InventoryItemResponse(
        Guid Id,
        Guid PropertyId,
        string Sku,
        string Name,
        decimal QuantityOnHand,
        DateTime CreatedAtUtc)
    {
        public static InventoryItemResponse From(InventoryItemAggregate inventoryItem)
        {
            return new InventoryItemResponse(
                inventoryItem.Id.Value,
                inventoryItem.PropertyId,
                inventoryItem.Sku,
                inventoryItem.Name,
                inventoryItem.QuantityOnHand,
                inventoryItem.CreatedAtUtc);
        }
    }
}
