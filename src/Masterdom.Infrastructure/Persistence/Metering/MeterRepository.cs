using Masterdom.Modules.Metering.Domain.Entities.Metering;
using Masterdom.Modules.Metering.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Masterdom.Infrastructure.Persistence.Metering;

public sealed class MeterRepository : IMeterRepository
{
    private readonly MasterdomDbContext _dbContext;

    public MeterRepository(MasterdomDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public void Add(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);
        _dbContext.Meters.Add(meter);
    }

    public Meter? GetById(MeterId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _dbContext.Meters
            .Include(x => x.HistoricalReadings)
            .FirstOrDefault(x => x.Id == id);
    }

    public Meter? GetByNumber(MeterNumber number)
    {
        ArgumentNullException.ThrowIfNull(number);

        return _dbContext.Meters
            .Include(x => x.HistoricalReadings)
            .FirstOrDefault(x => x.MeterNumber == number);
    }

    public void Update(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);
        _dbContext.Meters.Update(meter);
    }
}
