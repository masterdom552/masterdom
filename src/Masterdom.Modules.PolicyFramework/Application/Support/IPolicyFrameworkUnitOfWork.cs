namespace Masterdom.Modules.PolicyFramework.Application.Support;

public interface IPolicyFrameworkUnitOfWork
{
    void Execute(Action operation);
}
