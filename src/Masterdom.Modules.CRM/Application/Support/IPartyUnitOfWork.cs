namespace Masterdom.Modules.CRM.Application.Support;

/// <summary>
/// Defines the transactional persistence boundary for CRM party use-cases.
/// </summary>
public interface IPartyUnitOfWork
{
    void Execute(Action operation);
}
