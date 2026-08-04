using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Queries;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Modules.Metering.Application.Services;

public interface IMeteringApplicationService
{
    MeterAggregate InstallMeter(InstallMeterCommand command);

    MeterAggregate SubmitReading(SubmitReadingCommand command);

    MeterAggregate ApproveReading(ApproveReadingCommand command);

    MeterAggregate CorrectReading(CorrectReadingCommand command);

    MeterAggregate RetireMeter(RetireMeterCommand command);

    MeterAggregate? GetMeter(GetMeterByIdQuery query);

    MeterAggregate? GetMeterByNumber(GetMeterByNumberQuery query);
}
