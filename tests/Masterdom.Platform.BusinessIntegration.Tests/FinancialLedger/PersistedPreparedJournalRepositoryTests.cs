using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Infrastructure.Persistence;
using Masterdom.Infrastructure.Persistence.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Platform.Tests.FinancialLedger;

public sealed class PersistedPreparedJournalRepositoryTests
{
    [Fact]
    public void AddAndGetByPostingReference_ShouldRoundTripPreparedJournal()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MasterdomDbContext(options);
        var repository = new PersistedPreparedJournalRepository(dbContext);

        var ledgerId = LedgerId.New();
        var createdAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 0, 0), DateTimeKind.Utc);
        var persisted = PersistedPreparedJournal.Create(ledgerId, CreatePreparedJournal(), createdAtUtc);

        repository.Add(persisted);
        dbContext.SaveChanges();

        var loaded = repository.GetByPostingReference(ledgerId, persisted.PostingReference);

        Assert.NotNull(loaded);
        Assert.Equal(persisted.PersistenceId, loaded!.PersistenceId);
        Assert.Equal("USD", loaded.PreparedJournal.CurrencyCode);
        Assert.Equal(JournalLifecycleState.Prepared, loaded.LifecycleState);
    }

    [Fact]
    public void Update_ShouldPersistLifecycleTransition()
    {
        var options = new DbContextOptionsBuilder<MasterdomDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new MasterdomDbContext(options);
        var repository = new PersistedPreparedJournalRepository(dbContext);

        var ledgerId = LedgerId.New();
        var createdAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 0, 0), DateTimeKind.Utc);
        var validatedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 5, 0), DateTimeKind.Utc);

        var persisted = PersistedPreparedJournal.Create(ledgerId, CreatePreparedJournal(), createdAtUtc);
        repository.Add(persisted);
        dbContext.SaveChanges();

        var updated = persisted.MarkValidated(validatedAtUtc);
        repository.Update(updated);
        dbContext.SaveChanges();

        var loaded = repository.GetByPostingReference(ledgerId, persisted.PostingReference);

        Assert.NotNull(loaded);
        Assert.Equal(JournalLifecycleState.Validated, loaded!.LifecycleState);
        Assert.Equal(validatedAtUtc, loaded.ValidatedAtUtc);
    }

    private static PreparedJournal CreatePreparedJournal()
    {
        var lines = new List<PreparedJournalLine>
        {
            new("line-1", "1100", "Accounts Receivable", FinancialPostingDirection.Debit, 100m, "USD", "Debit"),
            new("line-2", "4100", "Rental Revenue", FinancialPostingDirection.Credit, 100m, "USD", "Credit")
        };

        return new PreparedJournal(
            "PREP-BILL-PLAT-001-20260831",
            "BILL:00000000000000000000000000000010",
            "JRN-BILLING-20260831-ABC123",
            new DateOnly(2026, 8, 31),
            "USD",
            "Prepared journal",
            "BATCH-202608",
            "billing",
            Guid.Parse("00000000-0000-0000-0000-000000000010"),
            "BILL-PLAT-001",
            lines,
            JournalLifecycleState.Prepared,
            null,
            null,
            null,
            null,
            null,
            new Dictionary<string, string>
            {
                ["billNumber"] = "BILL-PLAT-001"
            });
    }
}
