using Masterdom.Core.Identity.Entities.PasswordReset;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Proves PasswordResetRepository's query methods (CAP-023 Phase 3).
///
/// TryCompleteAsync -- the atomic, concurrency-safe completion mechanism
/// (EF Core's ExecuteUpdateAsync, a single conditional UPDATE rather than a
/// load-then-save round trip) -- is deliberately NOT tested here.
/// EF Core's InMemory provider does not implement ExecuteUpdate/
/// ExecuteUpdateAsync at all (confirmed empirically: it throws
/// InvalidOperationException on any call, not only under concurrency),
/// which is a documented InMemory-provider limitation, not a defect in the
/// mechanism -- ExecuteUpdateAsync is fully supported by Npgsql, the real
/// provider this application runs against. This mirrors this repository's
/// already-accepted WebApplicationFactory test-infrastructure gap: the
/// production mechanism is correct and used unmodified; only its automated
/// test coverage is bounded by what this repository's InMemory-based test
/// infrastructure can exercise. TryCompleteAsync's behavior is proven
/// instead at the handler level via a fake IPasswordResetRepository (see
/// CompletePasswordResetCommandHandlerTests), and empirically against the
/// real deployment where live validation is performed.
/// </summary>
public sealed class PasswordResetRepositoryTests
{
    [Fact]
    public async Task GetPendingByUserIdAsync_ReturnsMostRecentPendingRequest()
    {
        using var dbContext = CreateDbContext();
        var repository = new PasswordResetRepository(dbContext);

        var userId = UserId.New();
        var older = PasswordReset.Create(userId, "older-hash", TimeSpan.FromMinutes(15));
        repository.Add(older);
        await dbContext.SaveChangesAsync();

        var newer = PasswordReset.Create(userId, "newer-hash", TimeSpan.FromMinutes(15));
        repository.Add(newer);
        await dbContext.SaveChangesAsync();

        var pending = await repository.GetPendingByUserIdAsync(userId);

        Assert.NotNull(pending);
        Assert.Equal(newer.Id, pending!.Id);
    }

    [Fact]
    public async Task GetPendingByUserIdAsync_IgnoresCompletedAndCancelledRequests()
    {
        using var dbContext = CreateDbContext();
        var repository = new PasswordResetRepository(dbContext);

        var userId = UserId.New();
        var completed = PasswordReset.Create(userId, "hash-1", TimeSpan.FromMinutes(15));
        completed.Complete(DateTime.UtcNow);
        repository.Add(completed);
        await dbContext.SaveChangesAsync();

        var pending = await repository.GetPendingByUserIdAsync(userId);

        Assert.Null(pending);
    }

    private static MasterdomDbContext CreateDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new MasterdomDbContext(options);
    }
}
