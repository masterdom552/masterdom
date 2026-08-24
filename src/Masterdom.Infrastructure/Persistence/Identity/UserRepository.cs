using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core repository implementation for <see cref="User"/> reads needed by authentication.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly MasterdomDbContext _dbContext;

    public UserRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<User?> GetByUsernameAsync(Username username, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(username);

        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    }

    public async Task<Guid?> GetLinkedPersonIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var identityProfile = await _dbContext.IdentityProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == user.IdentityProfileId, cancellationToken);

        return identityProfile?.PersonId?.Value;
    }
}
