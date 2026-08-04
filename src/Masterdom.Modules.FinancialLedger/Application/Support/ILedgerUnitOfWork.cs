namespace Masterdom.Modules.FinancialLedger.Application.Support;

public interface ILedgerUnitOfWork
{
    void Execute(Action operation);
}
