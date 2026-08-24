using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Security;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Identity;

/// <summary>
/// EF Core repository implementation for <see cref="PasswordReset"/>.
/// </summary>
public sealed class PasswordResetRepository : IPasswordResetRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PasswordResetRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(PasswordReset passwordReset)
    {
        ArgumentNullException.ThrowIfNull(passwordReset);

        _dbContext.PasswordResets.Add(passwordReset);
    }

    public async Task<PasswordReset?> GetPendingByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userId);

        return await _dbContext.PasswordResets
            .Where(x => x.UserId == userId && x.Status == PasswordResetStatus.Pending)
            .OrderByDescending(x => x.RequestedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TryCompleteAsync(
        PasswordResetId id,
        DateTime completedAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        // A single conditional update, not a load-then-save round trip: two
        // concurrent completion attempts against the same request cannot
        // both succeed, since only one can match Status == Pending at the
        // moment its UPDATE executes.
        var rowsAffected = await _dbContext.PasswordResets
            .Where(x => x.Id == id && x.Status == PasswordResetStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, PasswordResetStatus.Completed)
                    .SetProperty(x => x.CompletedAtUtc, completedAtUtc),
                cancellationToken);

        return rowsAffected == 1;
    }
}
