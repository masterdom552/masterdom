using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Application.Commands;

public sealed record RetireMeterCommand(MeterId MeterId, RemovalDate RemovalDate);
