using Masterdom.Modules.Authentication.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core unit-of-work implementation for authentication application operations.
/// </summary>
public sealed class AuthenticationUnitOfWork : IAuthenticationUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public AuthenticationUnitOfWork(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
