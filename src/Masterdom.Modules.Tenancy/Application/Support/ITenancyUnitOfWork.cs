namespace Masterdom.Modules.Tenancy.Application.Support;

/// <summary>
/// Defines the transactional persistence boundary for tenancy use-cases.
/// </summary>
public interface ITenancyUnitOfWork
{
    void Execute(Action operation);
}
