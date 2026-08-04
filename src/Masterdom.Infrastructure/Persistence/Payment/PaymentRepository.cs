using Masterdom.Modules.Payment.Domain.Entities.Payment;
using Masterdom.Modules.Payment.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using PaymentAggregate = Masterdom.Modules.Payment.Domain.Entities.Payment.Payment;

namespace Masterdom.Infrastructure.Persistence.Payment;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly MasterdomDbContext _dbContext;

    public PaymentRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(PaymentAggregate payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        _dbContext.Payments.Add(payment);
    }

    public void Update(PaymentAggregate payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        _dbContext.Payments.Update(payment);
    }

    public PaymentAggregate? GetById(PaymentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.Payments
            .Include(x => x.Allocations)
            .Include(x => x.Versions)
            .Include(x => x.Receipts)
            .Include(x => x.Snapshots)
            .FirstOrDefault(x => x.Id == id);
    }

    public PaymentAggregate? GetByReference(PaymentReference paymentReference)
    {
        ArgumentNullException.ThrowIfNull(paymentReference);

        return _dbContext.Payments
            .AsEnumerable()
            .FirstOrDefault(x => x.PaymentReference == paymentReference);
    }
}
