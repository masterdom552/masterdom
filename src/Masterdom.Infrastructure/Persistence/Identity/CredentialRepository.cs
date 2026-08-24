using Masterdom.Core.Identity.Entities.Credential;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core repository implementation for <see cref="Credential"/>.
/// </summary>
public sealed class CredentialRepository : ICredentialRepository
{
    private readonly MasterdomDbContext _dbContext;

    public CredentialRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Credential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);

        return await _dbContext.Credentials
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public void Add(Credential credential)
    {
        ArgumentNullException.ThrowIfNull(credential);

        _dbContext.Credentials.Add(credential);
    }
}
