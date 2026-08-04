namespace Masterdom.Modules.Payment.Application.Support;

public interface ICommandHandler<in TCommand, out TResult>
{
    TResult Handle(TCommand command);
}
