namespace Masterdom.Modules.Inventory.Application.Support;

public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
