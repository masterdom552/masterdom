using Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger;
using Masterdom.Modules.FinancialLedger.Domain.Repositories;
using LedgerAggregate = Masterdom.Modules.FinancialLedger.Domain.Entities.FinancialLedger.Ledger;

namespace Masterdom.Infrastructure.Persistence.FinancialLedger;

public sealed class LedgerRepository : ILedgerRepository
{
    private readonly MasterdomDbContext _dbContext;

    public LedgerRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(LedgerAggregate ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        _dbContext.Ledgers.Add(ledger);
    }

    public void Update(LedgerAggregate ledger)
    {
        ArgumentNullException.ThrowIfNull(ledger);
        _dbContext.Ledgers.Update(ledger);
    }

    public LedgerAggregate? GetById(LedgerId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return _dbContext.Ledgers.FirstOrDefault(x => x.Id == id);
    }

    public LedgerAggregate? GetByCode(string ledgerCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerCode);
        return _dbContext.Ledgers.FirstOrDefault(x => x.LedgerCode == ledgerCode.Trim());
    }
}
