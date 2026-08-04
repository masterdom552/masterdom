using Masterdom.Modules.Payment.Application.Support;

namespace Masterdom.Infrastructure.Persistence.Payment;

public sealed class PaymentUnitOfWork : IPaymentUnitOfWork
{
    private readonly MasterdomDbContext _dbContext;

    public PaymentUnitOfWork(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Execute(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        using var transaction = _dbContext.Database.BeginTransaction();

        operation();
        _dbContext.SaveChanges();

        transaction.Commit();
    }
}
