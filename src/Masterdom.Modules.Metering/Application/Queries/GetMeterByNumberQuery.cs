using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Application.Queries;

public sealed record GetMeterByNumberQuery(MeterNumber MeterNumber);
