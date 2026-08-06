using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Support;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Host.Api;

internal static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var group = app.MapGroup("/api/inventory/items").WithTags("Inventory").RequireAuthorization();

        group.MapPost("/", CreateInventoryItem);

        return app;
    }

    internal static IResult CreateInventoryItem(
        CreateInventoryItemRequest request,
        ICommandHandler<CreateInventoryItemCommand, ExecutionResult<InventoryItemAggregate>> handler)
    {
        var command = new CreateInventoryItemCommand(
            request.PropertyId,
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

    internal sealed record CreateInventoryItemRequest(
        Guid PropertyId,
        string Sku,
        string Name,
        decimal QuantityOnHand,
        DateTime CreatedAtUtc);

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
