using Masterdom.Core.Identity.Entities.DelegatedAuthority;
using Masterdom.Core.Identity.Entities.Role;
using Masterdom.Core.Identity.Entities.User;
using Masterdom.Core.Identity.ValueObjects;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Infrastructure.Tests.Security;

/// <summary>
/// Proves DelegatedAuthorityRepository's corrected queries (CAP-023 Phase 4)
/// against a real relational EF Core provider (SQLite) -- unlike EF Core's
/// InMemory provider, SQLite performs genuine LINQ-to-SQL translation, so
/// these tests would fail with the same InvalidOperationException the
/// original .Value-inside-Where predicate produced against Npgsql in
/// production, if that predicate shape were restored.
///
/// This proves relational query TRANSLATION and EXECUTION. It does not
/// execute against PostgreSQL/Npgsql; that remains separately authorized,
/// unperformed work (see the governing package record).
/// </summary>
public sealed class DelegatedAuthorityRepositoryRelationalTests
{
    [Fact]
    public async Task GetActiveDelegationsAsync_WithMatchingActiveDelegation_ReturnsIt()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatedToUserId = UserId.New();
        var delegation = CreateDelegation(delegatedToUserId, effectiveFromUtc: DateTime.UtcNow.AddMinutes(-5));
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetActiveDelegationsAsync(delegatedToUserId.Value, DateTime.UtcNow);

        Assert.Single(result);
        Assert.Equal(delegation.Id, result.Single().Id);
    }

    [Fact]
    public async Task GetActiveDelegationsAsync_ForUnrelatedUser_DoesNotReturnDelegation()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatedToUserId = UserId.New();
        var unrelatedUserId = UserId.New();
        var delegation = CreateDelegation(delegatedToUserId, effectiveFromUtc: DateTime.UtcNow.AddMinutes(-5));
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetActiveDelegationsAsync(unrelatedUserId.Value, DateTime.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveDelegationsAsync_WithRevokedDelegation_ExcludesIt()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatedToUserId = UserId.New();
        var delegation = CreateDelegation(delegatedToUserId, effectiveFromUtc: DateTime.UtcNow.AddMinutes(-5));
        delegation.Revoke(UserId.New(), "test revocation");
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetActiveDelegationsAsync(delegatedToUserId.Value, DateTime.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveDelegationsAsync_WithExpiredDelegation_ExcludesIt()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatedToUserId = UserId.New();
        var delegation = CreateDelegation(
            delegatedToUserId,
            effectiveFromUtc: DateTime.UtcNow.AddDays(-2),
            effectiveToUtc: DateTime.UtcNow.AddDays(-1));
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetActiveDelegationsAsync(delegatedToUserId.Value, DateTime.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveDelegationsAsync_WithNotYetEffectiveDelegation_ExcludesIt()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatedToUserId = UserId.New();
        var delegation = CreateDelegation(delegatedToUserId, effectiveFromUtc: DateTime.UtcNow.AddDays(1));
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetActiveDelegationsAsync(delegatedToUserId.Value, DateTime.UtcNow);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDelegationsByDelegatorAsync_WithMatchingDelegator_ReturnsExpectedRecords()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatorUserId = UserId.New();
        var delegation = CreateDelegationFrom(delegatorUserId);
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetDelegationsByDelegatorAsync(delegatorUserId.Value);

        Assert.Single(result);
        Assert.Equal(delegation.Id, result.Single().Id);
    }

    [Fact]
    public async Task GetDelegationsByDelegatorAsync_ForUnrelatedDelegator_ExcludesRecords()
    {
        using var fixture = CreateSqliteContext();
        var repository = new DelegatedAuthorityRepository(fixture.DbContext);

        var delegatorUserId = UserId.New();
        var unrelatedDelegatorId = UserId.New();
        var delegation = CreateDelegationFrom(delegatorUserId);
        fixture.DbContext.DelegatedAuthorities.Add(delegation);
        await fixture.DbContext.SaveChangesAsync();

        var result = await repository.GetDelegationsByDelegatorAsync(unrelatedDelegatorId.Value);

        Assert.Empty(result);
    }

    private static DelegatedAuthority CreateDelegation(
        UserId delegatedToUserId,
        DateTime effectiveFromUtc,
        DateTime? effectiveToUtc = null)
    {
        return DelegatedAuthority.Create(
            delegatorUserId: UserId.New(),
            delegatedToUserId: delegatedToUserId,
            delegatedRoleId: RoleId.New(),
            scope: DelegationScope.Unrestricted(),
            effectiveFromUtc: effectiveFromUtc,
            effectiveToUtc: effectiveToUtc);
    }

    private static DelegatedAuthority CreateDelegationFrom(UserId delegatorUserId)
    {
        return DelegatedAuthority.Create(
            delegatorUserId: delegatorUserId,
            delegatedToUserId: UserId.New(),
            delegatedRoleId: RoleId.New(),
            scope: DelegationScope.Unrestricted(),
            effectiveFromUtc: DateTime.UtcNow.AddMinutes(-5),
            effectiveToUtc: DateTime.UtcNow.AddDays(1));
    }

    private static SqliteDbContextFixture CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new MasterdomDbContext(options);
        dbContext.Database.EnsureCreated();

        return new SqliteDbContextFixture(dbContext, connection);
    }

    private sealed class SqliteDbContextFixture : IDisposable
    {
        public SqliteDbContextFixture(MasterdomDbContext dbContext, SqliteConnection connection)
        {
            DbContext = dbContext;
            _connection = connection;
        }

        public MasterdomDbContext DbContext { get; }

        private readonly SqliteConnection _connection;

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Dispose();
        }
    }
}
