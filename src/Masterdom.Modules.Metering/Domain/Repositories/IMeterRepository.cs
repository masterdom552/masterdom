using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Domain.Repositories;

public interface IMeterRepository
{
    void Add(Meter meter);

    Meter? GetById(MeterId id);

    Meter? GetByNumber(MeterNumber number);

    void Update(Meter meter);
}
