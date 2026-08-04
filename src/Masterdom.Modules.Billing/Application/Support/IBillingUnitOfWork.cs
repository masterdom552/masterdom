namespace Masterdom.Modules.Billing.Application.Support;

/// <summary>
/// Defines transactional persistence boundary for billing use-cases.
/// </summary>
public interface IBillingUnitOfWork
{
    void Execute(Action operation);
}
