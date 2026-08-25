using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.Settlement;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Infrastructure.Tests.Persistence.Settlement;

public sealed class BillSettlementIntegrationTests
{
    [Fact]
    public void Create_PopulatesAllFields()
    {
        var allocationId = Guid.NewGuid();
        var billId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        var allocatedAt = DateTime.UtcNow;

        var settlement = BillSettlement.Create(
            allocationId, billId, "BILL-001", paymentId, "PAY-001", 500m, allocatedAt);

        Assert.NotEqual(Guid.Empty, settlement.Id);
        Assert.Equal(allocationId, settlement.AllocationId);
        Assert.Equal(billId, settlement.BillId);
        Assert.Equal("BILL-001", settlement.BillNumber);
        Assert.Equal(paymentId, settlement.PaymentId);
        Assert.Equal("PAY-001", settlement.PaymentReference);
        Assert.Equal(500m, settlement.Amount);
        Assert.Equal(allocatedAt, settlement.AllocatedAtUtc);
        Assert.False(settlement.IsReversed);
        Assert.Null(settlement.ReversedAtUtc);
        Assert.Null(settlement.ReversalReason);
    }

    [Fact]
    public void Reverse_SetsReversalFields()
    {
        var settlement = BillSettlement.Create(
            Guid.NewGuid(), Guid.NewGuid(), "BILL-001", Guid.NewGuid(), "PAY-001", 500m, DateTime.UtcNow);

        var reversedAt = DateTime.UtcNow.AddMinutes(5);
        settlement.Reverse("Tenant dispute", reversedAt);

        Assert.True(settlement.IsReversed);
        Assert.Equal(reversedAt, settlement.ReversedAtUtc);
        Assert.Equal("Tenant dispute", settlement.ReversalReason);
    }

    [Fact]
    public void Persist_AndRetrieve_RoundTripsCorrectly()
    {
        using var fixture = CreateSqliteContext();
        var dbContext = fixture.DbContext;

        var allocationId = Guid.NewGuid();
        var settlement = BillSettlement.Create(
            allocationId, Guid.NewGuid(), "BILL-002", Guid.NewGuid(), "PAY-002", 750.50m, DateTime.UtcNow);

        dbContext.BillSettlements.Add(settlement);
        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var loaded = dbContext.BillSettlements.Single(s => s.AllocationId == allocationId);

        Assert.Equal(settlement.Id, loaded.Id);
        Assert.Equal(allocationId, loaded.AllocationId);
        Assert.Equal("BILL-002", loaded.BillNumber);
        Assert.Equal("PAY-002", loaded.PaymentReference);
        Assert.Equal(750.50m, loaded.Amount);
        Assert.False(loaded.IsReversed);
    }

    [Fact]
    public void UniqueConstraint_OnAllocationId_PreventsInsertDuplicate()
    {
        using var fixture = CreateSqliteContext();
        var dbContext = fixture.DbContext;

        var allocationId = Guid.NewGuid();
        var first = BillSettlement.Create(
            allocationId, Guid.NewGuid(), "BILL-003", Guid.NewGuid(), "PAY-003", 100m, DateTime.UtcNow);

        dbContext.BillSettlements.Add(first);
        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var duplicate = BillSettlement.Create(
            allocationId, Guid.NewGuid(), "BILL-004", Guid.NewGuid(), "PAY-004", 200m, DateTime.UtcNow);

        dbContext.BillSettlements.Add(duplicate);

        Assert.Throws<DbUpdateException>(() => dbContext.SaveChanges());
    }

    [Fact]
    public void Persist_ReversedSettlement_RoundTripsReversalFields()
    {
        using var fixture = CreateSqliteContext();
        var dbContext = fixture.DbContext;

        var allocationId = Guid.NewGuid();
        var settlement = BillSettlement.Create(
            allocationId, Guid.NewGuid(), "BILL-005", Guid.NewGuid(), "PAY-005", 300m, DateTime.UtcNow);

        var reversedAt = new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);
        settlement.Reverse("Cancelled", reversedAt);

        dbContext.BillSettlements.Add(settlement);
        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var loaded = dbContext.BillSettlements.Single(s => s.AllocationId == allocationId);

        Assert.True(loaded.IsReversed);
        Assert.Equal(reversedAt, loaded.ReversedAtUtc);
        Assert.Equal("Cancelled", loaded.ReversalReason);
    }

    [Fact]
    public void MultipleSettlements_SameBillId_Allowed()
    {
        using var fixture = CreateSqliteContext();
        var dbContext = fixture.DbContext;

        var billId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        var s1 = BillSettlement.Create(Guid.NewGuid(), billId, "BILL-006", paymentId, "PAY-006", 150m, DateTime.UtcNow);
        var s2 = BillSettlement.Create(Guid.NewGuid(), billId, "BILL-006", paymentId, "PAY-006", 150m, DateTime.UtcNow);

        dbContext.BillSettlements.AddRange(s1, s2);
        dbContext.SaveChanges();

        var count = dbContext.BillSettlements.Count(s => s.BillId == billId);
        Assert.Equal(2, count);
    }

    [Fact]
    public void UpdateReversalInPlace_PersistsChanges()
    {
        using var fixture = CreateSqliteContext();
        var dbContext = fixture.DbContext;

        var allocationId = Guid.NewGuid();
        var settlement = BillSettlement.Create(
            allocationId, Guid.NewGuid(), "BILL-007", Guid.NewGuid(), "PAY-007", 400m, DateTime.UtcNow);

        dbContext.BillSettlements.Add(settlement);
        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var loaded = dbContext.BillSettlements.Single(s => s.AllocationId == allocationId);
        var reversedAt = new DateTime(2026, 8, 26, 9, 0, 0, DateTimeKind.Utc);
        loaded.Reverse("Payment error", reversedAt);
        dbContext.SaveChanges();
        dbContext.ChangeTracker.Clear();

        var reloaded = dbContext.BillSettlements.Single(s => s.AllocationId == allocationId);
        Assert.True(reloaded.IsReversed);
        Assert.Equal("Payment error", reloaded.ReversalReason);
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
        private readonly SqliteConnection _connection;

        public SqliteDbContextFixture(MasterdomDbContext dbContext, SqliteConnection connection)
        {
            DbContext = dbContext;
            _connection = connection;
        }

        public MasterdomDbContext DbContext { get; }

        public void Dispose()
        {
            DbContext.Dispose();
            _connection.Dispose();
        }
    }
}
