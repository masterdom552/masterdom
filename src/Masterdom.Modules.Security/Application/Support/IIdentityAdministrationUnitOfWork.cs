namespace Masterdom.Modules.Security.Application.Support;

public interface IIdentityAdministrationUnitOfWork
{
    void Execute(Action operation);
}
