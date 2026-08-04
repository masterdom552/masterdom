using Masterdom.Modules.Billing.Domain.Entities.Billing;
using Masterdom.Modules.Billing.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using BillAggregate = Masterdom.Modules.Billing.Domain.Entities.Billing.Bill;

namespace Masterdom.Infrastructure.Persistence.Billing;

/// <summary>
/// EF Core repository implementation for bill aggregates.
/// </summary>
public sealed class BillRepository : IBillRepository
{
    private readonly MasterdomDbContext _dbContext;

    public BillRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(BillAggregate bill)
    {
        ArgumentNullException.ThrowIfNull(bill);
        _dbContext.Bills.Add(bill);
    }

    public BillAggregate? GetById(BillId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.Bills
            .Include(x => x.Versions)
            .FirstOrDefault(x => x.Id == id);
    }

    public BillAggregate? GetByNumber(BillNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return _dbContext.Bills
            .Include(x => x.Versions)
            .FirstOrDefault(x => x.BillNumber == number);
    }

    public void Update(BillAggregate bill)
    {
        ArgumentNullException.ThrowIfNull(bill);
        _dbContext.Bills.Update(bill);
    }
}
