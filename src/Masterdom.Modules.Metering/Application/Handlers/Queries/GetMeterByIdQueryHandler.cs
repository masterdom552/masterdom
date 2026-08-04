using Masterdom.Modules.Metering.Application.Queries;
using Masterdom.Modules.Metering.Application.Services;
using Masterdom.Modules.Metering.Application.Support;
using MeterAggregate = Masterdom.Modules.Metering.Domain.Entities.Metering.Meter;

namespace Masterdom.Modules.Metering.Application.Handlers.Queries;

public sealed class GetMeterByIdQueryHandler : IQueryHandler<GetMeterByIdQuery, ExecutionResult<MeterAggregate>>
{
    private readonly IMeteringApplicationService _applicationService;

    public GetMeterByIdQueryHandler(IMeteringApplicationService applicationService)
    {
        _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
    }

    public ExecutionResult<MeterAggregate> Handle(GetMeterByIdQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var meter = _applicationService.GetMeter(query);
        return meter is null
            ? ExecutionResult<MeterAggregate>.Failure("not_found", $"Meter '{query.MeterId}' was not found.")
            : ExecutionResult<MeterAggregate>.Success(meter);
    }
}
