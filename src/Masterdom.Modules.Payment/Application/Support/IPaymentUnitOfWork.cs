namespace Masterdom.Modules.Payment.Application.Support;

public interface IPaymentUnitOfWork
{
    void Execute(Action operation);
}
