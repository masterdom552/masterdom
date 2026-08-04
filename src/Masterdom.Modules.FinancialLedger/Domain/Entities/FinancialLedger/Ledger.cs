using Masterdom.Core.Common.Events;
using Masterdom.Core.Common.Interfaces;
using Masterdom.Core.Primitives;
using Masterdom.Modules.FinancialLedger.Contracts.Billing;
using Masterdom.Modules.FinancialLedger.Contracts.Payment;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Events;

namespace Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

public sealed class Ledger : AggregateRoot<LedgerId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<LedgerAccount> _accounts = [];
    private readonly List<LedgerTransaction> _transactions = [];
    private readonly List<PostingBatch> _postingBatches = [];
    private readonly List<LedgerSnapshot> _snapshots = [];
    private readonly List<LedgerVersion> _versions = [];

    private Ledger(LedgerId id, string ledgerCode, string ledgerName, DateTime createdAtUtc)
        : base(id)
    {
        LedgerCode = ledgerCode;
        LedgerName = ledgerName;
        CreatedAtUtc = createdAtUtc;
    }

    public string LedgerCode { get; private set; }

    public string LedgerName { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<LedgerAccount> Accounts => _accounts.AsReadOnly();

    public IReadOnlyCollection<LedgerTransaction> Transactions => _transactions.AsReadOnly();

    public IReadOnlyCollection<PostingBatch> PostingBatches => _postingBatches.AsReadOnly();

    public IReadOnlyCollection<LedgerSnapshot> Snapshots => _snapshots.AsReadOnly();

    public IReadOnlyCollection<LedgerVersion> Versions => _versions.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public LedgerVersion CurrentVersion => _versions[^1];

    public static Ledger Open(LedgerId id, string ledgerCode, string ledgerName, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerName);

        if (createdAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger opening timestamp must be UTC.");
        }

        var ledger = new Ledger(id, ledgerCode.Trim(), ledgerName.Trim(), createdAtUtc);
        ledger.AppendVersion("Ledger opened.", createdAtUtc);
        return ledger;
    }

    public void AddAccount(LedgerAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (_accounts.Any(x => x.AccountReference == account.AccountReference))
        {
            return;
        }

        _accounts.Add(account);
    }

    public LedgerTransaction PostBillingTransaction(BillingLedgerPostingContract contract, DateTime postedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(contract);
        return PostTransaction(
            contract.PostingReference,
            contract.JournalNumber,
            contract.PostingDate,
            contract.Description,
            contract.BatchReference,
            "billing",
            contract.Lines.Select(x => JournalEntry.Create(
                AccountReference.Create(x.AccountCode, x.AccountName),
                MoneyAmount.Create(x.DebitAmount),
                MoneyAmount.Create(x.CreditAmount),
                x.Description)),
            postedAtUtc);
    }

    public LedgerTransaction PostPaymentTransaction(PaymentLedgerPostingContract contract, DateTime postedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(contract);
        return PostTransaction(
            contract.PostingReference,
            contract.JournalNumber,
            contract.PostingDate,
            contract.Description,
            contract.BatchReference,
            "payment",
            contract.Lines.Select(x => JournalEntry.Create(
                AccountReference.Create(x.AccountCode, x.AccountName),
                MoneyAmount.Create(x.DebitAmount),
                MoneyAmount.Create(x.CreditAmount),
                x.Description)),
            postedAtUtc);
    }

    public LedgerTransaction ReverseJournal(Guid transactionId, string reversalJournalNumber, string reason, DateTime reversedAtUtc)
    {
        if (transactionId == Guid.Empty)
        {
            throw new InvalidOperationException("Ledger transaction identifier cannot be empty.");
        }

        var existing = _transactions.FirstOrDefault(x => x.TransactionId == transactionId)
            ?? throw new InvalidOperationException("Ledger transaction was not found.");

        var reversal = existing.Reverse(reversalJournalNumber, reason, reversedAtUtc);
        _transactions.Add(reversal);

        AppendVersion($"Journal reversed: {existing.JournalNumber}.", reversedAtUtc);
        Raise(new JournalReversedDomainEvent(Id, reversal.JournalNumber, transactionId, reversedAtUtc));

        return reversal;
    }

    public void CompletePostingBatch(string batchReference, DateTime completedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(batchReference);

        var index = _postingBatches.FindIndex(x => string.Equals(x.BatchReference, batchReference.Trim(), StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            throw new InvalidOperationException("Posting batch was not found.");
        }

        _postingBatches[index] = _postingBatches[index].Complete(completedAtUtc);

        AppendVersion($"Posting batch completed: {batchReference.Trim()}.", completedAtUtc);
        Raise(new PostingBatchCompletedDomainEvent(Id, _postingBatches[index].BatchId, _postingBatches[index].BatchReference, completedAtUtc));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private LedgerTransaction PostTransaction(
        string postingReference,
        string journalNumber,
        DateOnly postingDate,
        string description,
        string batchReference,
        string sourceModule,
        IEnumerable<JournalEntry> entries,
        DateTime postedAtUtc)
    {
        if (postedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Ledger posting timestamp must be UTC.");
        }

        var normalizedPostingReference = postingReference.Trim();
        var normalizedJournalNumber = journalNumber.Trim();
        var normalizedDescription = description.Trim();
        var normalizedBatchReference = batchReference.Trim();
        var normalizedSourceModule = sourceModule.Trim().ToLowerInvariant();

        var materializedEntries = entries.ToList();

        var existingByPostingReference = _transactions.FirstOrDefault(x =>
            string.Equals(x.PostingReference.Value, normalizedPostingReference, StringComparison.OrdinalIgnoreCase));

        if (existingByPostingReference is not null)
        {
            if (!MatchesExisting(existingByPostingReference, normalizedJournalNumber, normalizedDescription, materializedEntries))
            {
                throw new InvalidOperationException("Posting reference already exists with different journal content.");
            }

            return existingByPostingReference;
        }

        var existingByJournalNumber = _transactions.FirstOrDefault(x =>
            string.Equals(x.JournalNumber, normalizedJournalNumber, StringComparison.OrdinalIgnoreCase));

        if (existingByJournalNumber is not null)
        {
            throw new InvalidOperationException("Journal number already exists in ledger.");
        }

        var journal = Journal.Create(normalizedJournalNumber, normalizedDescription, materializedEntries);
        var transaction = LedgerTransaction.Create(
            PostingReference.Create(normalizedPostingReference),
            normalizedSourceModule,
            PostingDate.Create(postingDate),
            journal,
            postedAtUtc);

        foreach (var entry in transaction.JournalEntries)
        {
            AddAccount(LedgerAccount.Open(entry.AccountReference, sourceModule, postedAtUtc));
        }

        _transactions.Add(transaction);

        var existingBatchIndex = _postingBatches.FindIndex(x => string.Equals(x.BatchReference, normalizedBatchReference, StringComparison.OrdinalIgnoreCase));
        if (existingBatchIndex >= 0)
        {
            _postingBatches[existingBatchIndex] = _postingBatches[existingBatchIndex].AppendTransaction(transaction.TransactionId);
        }
        else
        {
            _postingBatches.Add(PostingBatch.Create(normalizedBatchReference, normalizedSourceModule, transaction.TransactionId, postedAtUtc));
        }

        AppendVersion($"Journal posted: {journal.JournalNumber}.", postedAtUtc);
        Raise(new LedgerTransactionCreatedDomainEvent(Id, transaction.TransactionId, transaction.PostingReference.Value, postedAtUtc));
        Raise(new JournalPostedDomainEvent(Id, transaction.JournalNumber, postedAtUtc));

        return transaction;
    }

    private static bool MatchesExisting(
        LedgerTransaction existing,
        string journalNumber,
        string description,
        IReadOnlyCollection<JournalEntry> entries)
    {
        if (!string.Equals(existing.JournalNumber, journalNumber, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(existing.Description, description, StringComparison.Ordinal))
        {
            return false;
        }

        if (existing.JournalEntries.Count != entries.Count)
        {
            return false;
        }

        var expected = entries
            .OrderBy(x => x.AccountReference.AccountCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AccountReference.AccountName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.DebitAmount.Value)
            .ThenBy(x => x.CreditAmount.Value)
            .ThenBy(x => x.Description, StringComparer.Ordinal)
            .Select(x => new
            {
                x.AccountReference.AccountCode,
                x.AccountReference.AccountName,
                Debit = x.DebitAmount.Value,
                Credit = x.CreditAmount.Value,
                x.Description
            })
            .ToList();

        var actual = existing.JournalEntries
            .OrderBy(x => x.AccountReference.AccountCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AccountReference.AccountName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.DebitAmount.Value)
            .ThenBy(x => x.CreditAmount.Value)
            .ThenBy(x => x.Description, StringComparer.Ordinal)
            .Select(x => new
            {
                x.AccountReference.AccountCode,
                x.AccountReference.AccountName,
                Debit = x.DebitAmount.Value,
                Credit = x.CreditAmount.Value,
                x.Description
            })
            .ToList();

        return expected.SequenceEqual(actual);
    }

    private void AppendVersion(string changeReason, DateTime occurredAtUtc)
    {
        var versionNumber = _versions.Count + 1;
        var version = LedgerVersion.Create(versionNumber, changeReason, occurredAtUtc);
        var totalDebits = MoneyAmount.Create(_transactions.Sum(x => x.DebitTotal));
        var totalCredits = MoneyAmount.Create(_transactions.Sum(x => x.CreditTotal));
        var snapshot = LedgerSnapshot.Capture(versionNumber, _transactions.Count, _accounts.Count, totalDebits, totalCredits, occurredAtUtc);

        _versions.Add(version);
        _snapshots.Add(snapshot);

        Raise(new LedgerVersionCreatedDomainEvent(Id, versionNumber, changeReason, occurredAtUtc));
        Raise(new LedgerSnapshotCreatedDomainEvent(Id, snapshot.SnapshotId, versionNumber, occurredAtUtc));
    }

    private void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
