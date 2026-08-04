using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Masterdom.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tools.
/// </summary>
public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<MasterdomDbContext>
{
    public MasterdomDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MasterdomDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=masterdom;Username=postgres;Password=postgres");

        return new MasterdomDbContext(optionsBuilder.Options);
    }
}
