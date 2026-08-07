using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence;

public static class DbContextUnitOfWorkExecutor
{
    public static void Execute(MasterdomDbContext dbContext, Action operation)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(operation);

        if (dbContext.Database.IsRelational())
        {
            using var transaction = dbContext.Database.BeginTransaction();
            operation();
            dbContext.SaveChanges();
            transaction.Commit();
            return;
        }

        operation();
        dbContext.SaveChanges();
    }
}
