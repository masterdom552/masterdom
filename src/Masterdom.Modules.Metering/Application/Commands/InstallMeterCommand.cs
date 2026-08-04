using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Application.Commands;

public sealed record InstallMeterCommand(
    MeterNumber MeterNumber,
    MeterCategory MeterCategory,
    MeterType MeterType,
    MeterLocationReference MeterLocationReference,
    InstallationDate InstallationDate);
