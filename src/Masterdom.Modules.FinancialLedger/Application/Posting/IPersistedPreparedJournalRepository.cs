using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;

namespace Masterdom.Modules.FinancialLedger.Application.Posting;

internal interface IPersistedPreparedJournalRepository
{
    PersistedPreparedJournal? GetByPostingReference(LedgerId ledgerId, string postingReference);

    void Add(PersistedPreparedJournal journal);

    void Update(PersistedPreparedJournal journal);
}
