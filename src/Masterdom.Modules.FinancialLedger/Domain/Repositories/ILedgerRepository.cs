using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Modules.FinancialLedger.Domain.Repositories;

public interface ILedgerRepository
{
    void Add(LedgerAggregate ledger);

    void Update(LedgerAggregate ledger);

    LedgerAggregate? GetById(LedgerId id);

    LedgerAggregate? GetByCode(string ledgerCode);
}
