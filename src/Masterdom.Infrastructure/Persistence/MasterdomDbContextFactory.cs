using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Masterdom.Infrastructure.Persistence;

/// <summary>
/// Used only by Entity Framework Core design-time tools (migrations).
/// This factory is not used by the application at runtime.
/// </summary>

///
/// <summary>
/// Creates a MasterdomDbContext during design time for EF Core tools.
/// </summary>
public sealed class MasterdomDbContextFactory
    : IDesignTimeDbContextFactory<MasterdomDbContext>
{
    public MasterdomDbContext CreateDbContext(string[] args)
    {
        var connectionString =
     Environment.GetEnvironmentVariable("MASTERDOM_CONNECTION_STRING")
     ?? "Host=localhost;Port=5432;Database=masterdom;Username=kady";

        var optionsBuilder =
            new DbContextOptionsBuilder<MasterdomDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new MasterdomDbContext(optionsBuilder.Options);
    }
}
