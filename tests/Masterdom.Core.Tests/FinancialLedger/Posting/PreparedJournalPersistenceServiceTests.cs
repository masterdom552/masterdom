using Masterdom.Abstractions.Financial.Posting;
using Masterdom.Modules.FinancialLedger.Application.Posting;
using Masterdom.Modules.FinancialLedger.Application.Support;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;

namespace Masterdom.Core.Tests.FinancialLedger.Posting;

public sealed class PreparedJournalPersistenceServiceTests
{
    [Fact]
    public void PersistAndPost_ShouldCreateLedgerTransaction_AndPersistPostedState()
    {
        var ledger = Ledger.Open(LedgerId.New(), "GL-PERSIST-1", "Persistence Ledger", DateTime.UtcNow);
        var ledgerRepository = new InMemoryLedgerRepository(ledger);
        var preparedRepository = new InMemoryPreparedJournalRepository();
        var service = new PreparedJournalPersistenceService(
            ledgerRepository,
            new PassThroughLedgerUnitOfWork(),
            preparedRepository);

        var result = service.PersistAndPost(
            ledger.Id,
            CreatePreparedJournal(),
            DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 10, 0), DateTimeKind.Utc));

        Assert.False(result.WasIdempotentReplay);
        Assert.Equal(JournalLifecycleState.Posted, result.State);
        Assert.Single(ledger.Transactions);

        var persisted = preparedRepository.GetByPostingReference(ledger.Id, "BILL:00000000000000000000000000000020");
        Assert.NotNull(persisted);
        Assert.Equal(JournalLifecycleState.Posted, persisted!.LifecycleState);
    }

    [Fact]
    public void PersistAndPost_ShouldThrow_WhenLedgerRejectsDuplicateJournalNumber()
    {
        var ledger = Ledger.Open(LedgerId.New(), "GL-PERSIST-3", "Persistence Ledger", DateTime.UtcNow);
        var ledgerRepository = new InMemoryLedgerRepository(ledger);
        var preparedRepository = new InMemoryPreparedJournalRepository();
        var service = new PreparedJournalPersistenceService(
            ledgerRepository,
            new PassThroughLedgerUnitOfWork(),
            preparedRepository);

        var postedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 10, 0), DateTimeKind.Utc);

        service.PersistAndPost(ledger.Id, CreatePreparedJournal("BILL:00000000000000000000000000000030", "JRN-BILLING-20260831-DUPLICATE"), postedAtUtc);

        Assert.Throws<InvalidOperationException>(() =>
            service.PersistAndPost(
                ledger.Id,
                CreatePreparedJournal("BILL:00000000000000000000000000000031", "JRN-BILLING-20260831-DUPLICATE"),
                postedAtUtc));
    }

    [Fact]
    public void PersistAndPost_ShouldPropagateRepositoryWriteFailure()
    {
        var ledger = Ledger.Open(LedgerId.New(), "GL-PERSIST-4", "Persistence Ledger", DateTime.UtcNow);
        var ledgerRepository = new InMemoryLedgerRepository(ledger);
        var preparedRepository = new ThrowingPreparedJournalRepository();
        var service = new PreparedJournalPersistenceService(
            ledgerRepository,
            new PassThroughLedgerUnitOfWork(),
            preparedRepository);

        var postedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 10, 0), DateTimeKind.Utc);

        Assert.Throws<InvalidOperationException>(() =>
            service.PersistAndPost(ledger.Id, CreatePreparedJournal(), postedAtUtc));
    }

    [Fact]
    public async Task PersistAndPost_ShouldAllowOnlyOneConcurrentAttempt()
    {
        var ledger = Ledger.Open(LedgerId.New(), "GL-PERSIST-5", "Persistence Ledger", DateTime.UtcNow);
        var ledgerRepository = new InMemoryLedgerRepository(ledger);
        var preparedRepository = new CoordinatedPreparedJournalRepository();
        var service = new PreparedJournalPersistenceService(
            ledgerRepository,
            new PassThroughLedgerUnitOfWork(),
            preparedRepository);

        var postedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 10, 0), DateTimeKind.Utc);

        var firstAttempt = Task.Run(() => service.PersistAndPost(ledger.Id, CreatePreparedJournal(), postedAtUtc));
        var secondAttempt = Task.Run(() => service.PersistAndPost(ledger.Id, CreatePreparedJournal(), postedAtUtc));

        try
        {
            await Task.WhenAll(firstAttempt, secondAttempt);
        }
        catch
        {
            // One concurrent attempt is expected to fail the repository uniqueness guard.
        }

        Assert.Equal(1, ledger.Transactions.Count);
        Assert.Equal(1, new[] { firstAttempt, secondAttempt }.Count(x => x.IsCompletedSuccessfully));
        Assert.Equal(1, new[] { firstAttempt, secondAttempt }.Count(x => x.IsFaulted));
    }

    [Fact]
    public void PersistAndPost_ShouldReturnReplay_WhenAlreadyPosted()
    {
        var ledger = Ledger.Open(LedgerId.New(), "GL-PERSIST-2", "Persistence Ledger", DateTime.UtcNow);
        var ledgerRepository = new InMemoryLedgerRepository(ledger);
        var preparedRepository = new InMemoryPreparedJournalRepository();
        var service = new PreparedJournalPersistenceService(
            ledgerRepository,
            new PassThroughLedgerUnitOfWork(),
            preparedRepository);

        var postedAtUtc = DateTime.SpecifyKind(new DateTime(2026, 8, 31, 1, 10, 0), DateTimeKind.Utc);
        service.PersistAndPost(ledger.Id, CreatePreparedJournal(), postedAtUtc);
        var replay = service.PersistAndPost(ledger.Id, CreatePreparedJournal(), postedAtUtc);

        Assert.True(replay.WasIdempotentReplay);
        Assert.Single(ledger.Transactions);
    }

    private static PreparedJournal CreatePreparedJournal()
    {
        return CreatePreparedJournal("BILL:00000000000000000000000000000020", "JRN-BILLING-20260831-ABC123");
    }

    private static PreparedJournal CreatePreparedJournal(string postingReference, string journalNumber)
    {
        var lines = new List<PreparedJournalLine>
        {
            new("line-1", "1100", "Accounts Receivable", FinancialPostingDirection.Debit, 200m, "USD", "Debit"),
            new("line-2", "4100", "Rental Revenue", FinancialPostingDirection.Credit, 200m, "USD", "Credit")
        };

        return new PreparedJournal(
            "PREP-BILL-CORE-001-20260831",
            postingReference,
            journalNumber,
            new DateOnly(2026, 8, 31),
            "USD",
            "Prepared journal",
            "BATCH-202608",
            "billing",
            Guid.Parse("00000000-0000-0000-0000-000000000020"),
            "BILL-CORE-001",
            lines);
    }

    private sealed class ThrowingPreparedJournalRepository : IPersistedPreparedJournalRepository
    {
        public PersistedPreparedJournal? GetByPostingReference(LedgerId ledgerId, string postingReference)
        {
            return null;
        }

        public void Add(PersistedPreparedJournal journal)
        {
            throw new InvalidOperationException("Prepared journal write failed.");
        }

        public void Update(PersistedPreparedJournal journal)
        {
            throw new InvalidOperationException("Prepared journal write failed.");
        }
    }

    private sealed class CoordinatedPreparedJournalRepository : IPersistedPreparedJournalRepository
    {
        private readonly Barrier _barrier = new(2);
        private readonly Dictionary<string, PersistedPreparedJournal> _items = new(StringComparer.OrdinalIgnoreCase);

        public PersistedPreparedJournal? GetByPostingReference(LedgerId ledgerId, string postingReference)
        {
            _barrier.SignalAndWait();
            return null;
        }

        public void Add(PersistedPreparedJournal journal)
        {
            var key = Key(journal.LedgerId, journal.PostingReference);

            if (!_items.TryAdd(key, journal))
            {
                throw new InvalidOperationException($"Prepared journal '{journal.PostingReference}' already exists.");
            }
        }

        public void Update(PersistedPreparedJournal journal)
        {
            _items[Key(journal.LedgerId, journal.PostingReference)] = journal;
        }

        private static string Key(LedgerId ledgerId, string postingReference)
        {
            return $"{ledgerId.Value:N}:{postingReference.Trim().ToUpperInvariant()}";
        }
    }

    private sealed class InMemoryLedgerRepository : ILedgerRepository
    {
        private readonly Ledger _ledger;

        public InMemoryLedgerRepository(Ledger ledger)
        {
            _ledger = ledger;
        }

        public void Add(Ledger ledger)
        {
            throw new NotSupportedException();
        }

        public void Update(Ledger ledger)
        {
            // No-op for in-memory test repository.
        }

        public Ledger? GetById(LedgerId id)
        {
            return _ledger.Id == id ? _ledger : null;
        }

        public Ledger? GetByCode(string ledgerCode)
        {
            return string.Equals(_ledger.LedgerCode, ledgerCode, StringComparison.OrdinalIgnoreCase)
                ? _ledger
                : null;
        }
    }

    private sealed class PassThroughLedgerUnitOfWork : ILedgerUnitOfWork
    {
        public void Execute(Action operation)
        {
            operation();
        }
    }

    private sealed class InMemoryPreparedJournalRepository : IPersistedPreparedJournalRepository
    {
        private readonly Dictionary<string, PersistedPreparedJournal> _items = new(StringComparer.OrdinalIgnoreCase);

        public PersistedPreparedJournal? GetByPostingReference(LedgerId ledgerId, string postingReference)
        {
            _items.TryGetValue(Key(ledgerId, postingReference), out var found);
            return found;
        }

        public void Add(PersistedPreparedJournal journal)
        {
            _items[Key(journal.LedgerId, journal.PostingReference)] = journal;
        }

        public void Update(PersistedPreparedJournal journal)
        {
            _items[Key(journal.LedgerId, journal.PostingReference)] = journal;
        }

        private static string Key(LedgerId ledgerId, string postingReference)
        {
            return $"{ledgerId.Value:N}:{postingReference.Trim().ToUpperInvariant()}";
        }
    }
}
