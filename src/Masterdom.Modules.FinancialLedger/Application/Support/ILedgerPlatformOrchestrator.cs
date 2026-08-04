using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Modules.FinancialLedger.Application.Support;

public interface ILedgerPlatformOrchestrator
{
    void OnLedgerMutated(LedgerAggregate ledger, string operationName);
}
