namespace Masterdom.Modules.Inventory.Application.Support;

public interface IInventoryUnitOfWork
{
    void Execute(Action operation);
}
