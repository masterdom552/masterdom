using Masterdom.Modules.FinancialLedger.Application.Support;
using Masterdom.Modules.FinancialLedger.Contracts.Billing;
using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal sealed class PreparedJournalPersistenceService
{
    private readonly ILedgerRepository _ledgerRepository;
    private readonly ILedgerUnitOfWork _unitOfWork;
    private readonly IPersistedPreparedJournalRepository _preparedJournalRepository;

    public PreparedJournalPersistenceService(
        ILedgerRepository ledgerRepository,
        ILedgerUnitOfWork unitOfWork,
        IPersistedPreparedJournalRepository preparedJournalRepository)
    {
        _ledgerRepository = ledgerRepository ?? throw new ArgumentNullException(nameof(ledgerRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _preparedJournalRepository = preparedJournalRepository ?? throw new ArgumentNullException(nameof(preparedJournalRepository));
    }

    public PreparedJournalPersistenceResult PersistAndPost(
        LedgerId ledgerId,
        PreparedJournal preparedJournal,
        DateTime postedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(ledgerId);
        ArgumentNullException.ThrowIfNull(preparedJournal);

        if (postedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidOperationException("Journal posting timestamp must be UTC.");
        }

        PreparedJournalPersistenceResult? result = null;

        _unitOfWork.Execute(() =>
        {
            var ledger = _ledgerRepository.GetById(ledgerId)
                ?? throw new InvalidOperationException($"Ledger '{ledgerId}' was not found.");

            var existing = _preparedJournalRepository.GetByPostingReference(ledgerId, preparedJournal.PostingReference);
            if (existing is not null && existing.LifecycleState == JournalLifecycleState.Posted)
            {
                result = new PreparedJournalPersistenceResult(
                    existing.PersistenceId,
                    existing.LedgerTransactionId,
                    existing.PostingReference,
                    existing.JournalNumber,
                    existing.LifecycleState,
                    true);

                return;
            }

            var working = existing ?? PersistedPreparedJournal.Create(ledgerId, preparedJournal, postedAtUtc);
            if (existing is null)
            {
                _preparedJournalRepository.Add(working);
            }

            if (working.LifecycleState == JournalLifecycleState.Prepared)
            {
                working = working.MarkValidated(postedAtUtc);
                _preparedJournalRepository.Update(working);
            }

            var contract = MapToLegacyPostingContract(working.PreparedJournal);
            var transaction = ledger.PostBillingTransaction(contract, postedAtUtc);

            working = working.MarkPosted(transaction.TransactionId, postedAtUtc);
            _preparedJournalRepository.Update(working);

            _ledgerRepository.Update(ledger);

            result = new PreparedJournalPersistenceResult(
                working.PersistenceId,
                transaction.TransactionId,
                working.PostingReference,
                working.JournalNumber,
                working.LifecycleState,
                false);
        });

        return result ?? throw new InvalidOperationException("Prepared journal persistence result was not produced.");
    }

    private static BillingLedgerPostingContract MapToLegacyPostingContract(PreparedJournal journal)
    {
        var lines = journal.Lines
            .Select(x => new LedgerPostingLineContract(
                x.AccountCode,
                x.AccountName,
                x.Direction == Masterdom.Abstractions.Financial.Posting.FinancialPostingDirection.Debit ? x.Amount : 0m,
                x.Direction == Masterdom.Abstractions.Financial.Posting.FinancialPostingDirection.Credit ? x.Amount : 0m,
                x.Description))
            .ToList()
            .AsReadOnly();

        return new BillingLedgerPostingContract(
            journal.PostingReference,
            journal.JournalNumber,
            journal.PostingDate,
            journal.Description,
            journal.BatchReference,
            lines);
    }
}
