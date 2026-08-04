namespace Masterdom.Modules.Properties.Application.Support;

/// <summary>
/// Defines the transactional persistence boundary for property use-cases.
/// </summary>
public interface IPropertyUnitOfWork
{
    void Execute(Action operation);
}
