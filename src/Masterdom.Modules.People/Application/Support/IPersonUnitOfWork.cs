namespace Masterdom.Modules.People.Application.Support;

/// <summary>
/// Defines the transactional persistence boundary for people use-cases.
/// </summary>
public interface IPersonUnitOfWork
{
    void Execute(Action operation);
}
