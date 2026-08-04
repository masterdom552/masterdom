using Masterdom.Modules.FinancialLedger.Application.Commands;
using Masterdom.Modules.FinancialLedger.Application.Queries;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Modules.FinancialLedger.Application.Services;

public interface ILedgerApplicationService
{
    LedgerAggregate OpenLedger(OpenLedgerCommand command);

    LedgerAggregate PostBillingJournal(PostBillingJournalCommand command);

    LedgerAggregate PostPaymentJournal(PostPaymentJournalCommand command);

    LedgerAggregate ReverseJournal(ReverseJournalCommand command);

    LedgerAggregate CompletePostingBatch(CompletePostingBatchCommand command);

    LedgerAggregate? GetLedger(GetLedgerByIdQuery query);

    LedgerAggregate? GetLedger(GetLedgerByCodeQuery query);
}
