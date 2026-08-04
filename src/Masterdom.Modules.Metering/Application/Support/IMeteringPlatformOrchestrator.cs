using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Modules.Metering.Application.Support;

public interface IMeteringPlatformOrchestrator
{
    void OnMeterMutated(MeterAggregate meter, string operationName);
}
