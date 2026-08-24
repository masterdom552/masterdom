namespace Masterdom.Modules.Authentication.Application.Support;

/// <summary>
/// Defines the persistence-commit boundary for authentication use-cases.
/// </summary>
public interface IAuthenticationUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
