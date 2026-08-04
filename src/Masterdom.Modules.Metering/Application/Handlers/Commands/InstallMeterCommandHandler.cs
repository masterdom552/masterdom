using Masterdom.Modules.Metering.Application.Commands;
using Masterdom.Modules.Metering.Application.Services;
using Masterdom.Modules.Metering.Application.Support;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Modules.Metering.Application.Handlers.Commands;

public sealed class InstallMeterCommandHandler : ICommandHandler<InstallMeterCommand, ExecutionResult<MeterAggregate>>
{
    private readonly IMeteringApplicationService _applicationService;

    public InstallMeterCommandHandler(IMeteringApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<MeterAggregate> Handle(InstallMeterCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            var meter = _applicationService.InstallMeter(command);
            return ExecutionResult<MeterAggregate>.Success(meter);
        }
        catch (ArgumentException ex)
        {
            return ExecutionResult<MeterAggregate>.Failure("validation_failed", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResult<MeterAggregate>.Failure("conflict", ex.Message);
        }
    }
}
