namespace Masterdom.Modules.Lease.Application.Support;

/// <summary>
/// Defines transactional persistence boundary for lease use-cases.
/// </summary>
public interface ILeaseUnitOfWork
{
    void Execute(Action operation);
}
