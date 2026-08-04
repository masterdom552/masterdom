namespace Masterdom.Modules.Metering.Application.Support;

public interface IMeteringUnitOfWork
{
    void Execute(Action operation);
}
