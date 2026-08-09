using Masterdom.Modules.Inventory.Application.Commands;
using Masterdom.Modules.Inventory.Application.Services;
using Masterdom.Modules.Inventory.Application.Support;
using InventoryItemAggregate = Masterdom.Modules.Inventory.Domain.Entities.Inventory.InventoryItem;

namespace Masterdom.Modules.Inventory.Application.Handlers.Commands;

public sealed class AdjustStockCommandHandler : ICommandHandler<AdjustStockCommand, ExecutionResult<InventoryItemAggregate>>
{
    private readonly IInventoryApplicationService _applicationService;

    public AdjustStockCommandHandler(IInventoryApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<InventoryItemAggregate> Handle(AdjustStockCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var inventoryItem = _applicationService.AdjustStock(command);
            return ExecutionResult<InventoryItemAggregate>.Success(inventoryItem);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ExecutionResult<InventoryItemAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<InventoryItemAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<InventoryItemAggregate>.Failure("conflict", ex.Message);
        }
    }
}
