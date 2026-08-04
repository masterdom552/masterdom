using Masterdom.Modules.Metering.Domain.Entities.Metering;

namespace Masterdom.Modules.Metering.Application.Commands;

public sealed record ApproveReadingCommand(
    MeterId MeterId,
    Guid ReadingId,
    ReviewedBy ReviewedBy,
    ReviewDate ReviewDate);
